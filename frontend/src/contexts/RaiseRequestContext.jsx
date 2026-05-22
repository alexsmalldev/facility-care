import React, { createContext, useContext, useEffect, useState } from 'react';
import {
    Combobox, ComboboxInput, ComboboxOption, ComboboxOptions,
    Dialog, DialogPanel, DialogBackdrop, DialogTitle
} from '@headlessui/react';
import { MagnifyingGlassIcon, ChevronRightIcon, TicketIcon, ChevronDownIcon, CheckIcon } from '@heroicons/react/20/solid';
import { ExclamationTriangleIcon, MapPinIcon } from '@heroicons/react/24/outline';
import { Listbox, ListboxButton, ListboxOptions, ListboxOption } from '@headlessui/react';
import { ExclamationCircleIcon } from '@heroicons/react/24/outline';
import { motion, AnimatePresence } from 'framer-motion';
import { useLocation, useNavigate } from 'react-router-dom';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { loadStripe } from '@stripe/stripe-js';
import { Elements, CardElement, useStripe, useElements } from '@stripe/react-stripe-js';

import { cn } from '../lib/utils';
import api from '../api/apiConfig';
import { useLoading } from './LoadingContext';
import { useNotification } from './NotificationContext';
import { useUser } from './UserContext';

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

const defaultInputBorder = "bg-white block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6";
const errorBorder = "block w-full rounded-md border-0 py-1.5 pr-10 text-red-900 ring-1 ring-inset ring-red-300 placeholder:text-red-300 focus:ring-2 focus:ring-inset focus:ring-red-500 sm:text-sm sm:leading-6";

const ServiceRequestSchema = Yup.object().shape({
    customerNotes: Yup.string().optional(),
    priority: Yup.string().required('Priority is required'),
    serviceTypeId: Yup.number().required('Service Item is required'),
    buildingId: Yup.number().required('Service Location is required')
});

const RaiseRequestContext = createContext();
export const useRaiseRequest = () => useContext(RaiseRequestContext);

const PaymentForm = ({ clientSecret, selectedService, onSuccess, onBack }) => {
    const stripe = useStripe();
    const elements = useElements();
    const { showLoading, hideLoading } = useLoading();
    const { triggerNotification } = useNotification();
    const [isProcessing, setIsProcessing] = useState(false);

    const handlePayment = async (e) => {
        e.preventDefault();
        if (!stripe || !elements) return;

        setIsProcessing(true);
        showLoading('Processing Payment...');

        try {
            const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
                payment_method: {
                    card: elements.getElement(CardElement)
                }
            });

            if (error) {
                triggerNotification('FAIL', 'Payment Failed', error.message);
            } else if (paymentIntent.status === 'succeeded') {
                onSuccess();
            }
        } catch (err) {
            triggerNotification('FAIL', 'Payment Failed', err.message);
        } finally {
            setIsProcessing(false);
            hideLoading();
        }
    };

    return (
        <div>
            <div className="mb-6 p-4 bg-white rounded-lg">
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <img src={selectedService?.serviceIcon} className="h-12 w-12" alt={selectedService?.name} />
                        <div>
                            <p className="text-sm font-medium text-gray-900">{selectedService?.name}</p>
                            <p className="text-sm text-gray-500">{selectedService?.description}</p>
                        </div>
                    </div>
                    <span className="text-lg font-semibold text-indigo-600">£{selectedService?.price?.toFixed(2)}</span>
                </div>
            </div>

            <form onSubmit={handlePayment} className="grid grid-cols-1 gap-y-6">
                <div>
                    <label className="block text-sm font-medium leading-6 text-gray-900 mb-2">
                        Card Details
                    </label>
                    <div className="p-3 bg-white rounded-md ring-1 ring-inset ring-gray-300">
                        <CardElement options={{
                            style: {
                                base: {
                                    fontSize: '16px',
                                    color: '#111827',
                                    '::placeholder': { color: '#9CA3AF' }
                                }
                            }
                        }} />
                    </div>
                </div>

                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                    <button
                        type="submit"
                        disabled={!stripe || isProcessing}
                        className={cn(
                            "inline-flex w-full justify-center rounded-md px-3 py-2 text-sm font-semibold shadow-sm sm:col-start-2",
                            stripe && !isProcessing
                                ? "bg-indigo-600 text-white hover:bg-indigo-500"
                                : "bg-indigo-400 text-gray-200 cursor-not-allowed"
                        )}
                    >
                        {isProcessing ? 'Processing...' : `Pay £${selectedService?.price?.toFixed(2)}`}
                    </button>
                    <button
                        type="button"
                        onClick={onBack}
                        className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:col-start-1 sm:mt-0"
                    >
                        Back
                    </button>
                </div>
            </form>
        </div>
    );
};

export const RaiseRequestProvider = ({ children }) => {
    const { showLoading, hideLoading } = useLoading();
    const location = useLocation();
    const navigate = useNavigate();
    const { user } = useUser();
    const { triggerNotification } = useNotification();

    const [availableServices, setAvailableServices] = useState([]);
    const [filteredServices, setFilteredServices] = useState([]);
    const [query, setQuery] = useState('');
    const [selectedService, setSelectedService] = useState(null);
    const [openSearch, setOpenSearch] = useState(false);
    const [openRequestRaiseDialog, setOpenRequestRaiseDialog] = useState(false);
    const [fetchUserRequests, setFetchUserRequests] = useState(null);
    const [step, setStep] = useState('form');
    const [clientSecret, setClientSecret] = useState('');
    const [pendingValues, setPendingValues] = useState(null);

    const initialValues = pendingValues || {
        customerNotes: '',
        priority: '',
        serviceTypeId: selectedService?.id,
        buildingId: ''
    };

    const priorityOptions = [
        { id: 'Low', name: 'Low' },
        { id: 'Medium', name: 'Medium' },
        { id: 'High', name: 'High' },
    ];

    const handleServiceTypeSelect = (item) => {
        setSelectedService(item);
        handleRequestSearchOnClose();
        setOpenRequestRaiseDialog(true);
    };

    const handleRequestSearchOnClose = () => {
        setOpenSearch(false);
        setFilteredServices([]);
        setAvailableServices([]);
        setQuery('');
    };

    const handleRaiseRequestDialogOnClose = () => {
        setOpenRequestRaiseDialog(false);
        setTimeout(() => {
            setSelectedService(null);
            setStep('form');
            setClientSecret('');
            setPendingValues(null);
        }, 300);
    };

    const handleSubmit = async (values, { setSubmitting }) => {
        showLoading('Processing...');
        try {
            if (selectedService?.isPaid) {
                const paymentResponse = await api.post('/ServiceRequests/create-payment-intent', values);
                setClientSecret(paymentResponse.data.clientSecret);
                setPendingValues(values);
                setStep('payment');
            } else {
                await api.post('/ServiceRequests', values);
                handleRaiseRequestDialogOnClose();
                if (location.pathname === '/my-requests' && fetchUserRequests) {
                    fetchUserRequests();
                } else {
                    navigate('/my-requests');
                }
                triggerNotification('SUCCESS', 'Service Request Raised Successfully');
            }
        } catch (e) {
            triggerNotification('FAIL', 'Failed to Raise Request', e.response?.data);
            setOpenRequestRaiseDialog(false);
        } finally {
            setSubmitting(false);
            hideLoading();
        }
    };

    const handlePaymentSuccess = async () => {
        try {
            showLoading('Creating Service Request...');
            await api.post('/ServiceRequests', pendingValues);
            handleRaiseRequestDialogOnClose();
            if (location.pathname === '/my-requests' && fetchUserRequests) {
                fetchUserRequests();
            } else {
                navigate('/my-requests');
            }
            triggerNotification('SUCCESS', 'Payment successful! Service Request Raised.');
        } catch (e) {
            triggerNotification('FAIL', 'Failed to create request after payment', e.response?.data);
        } finally {
            hideLoading();
        }
    };

    const handlePaymentBack = () => {
        setStep('form');
        setClientSecret('');
    };

    useEffect(() => {
        if (query.trim() === '') {
            setFilteredServices(availableServices);
        } else {
            setFilteredServices(availableServices.filter((item) =>
                item.name.toLowerCase().includes(query.toLowerCase()) ||
                item.description.toLowerCase().includes(query.toLowerCase())
            ));
        }
    }, [query, availableServices]);

    const showSearch = async () => {
        showLoading('Fetching Service Types...');
        try {
            const response = await api.get('/ServiceTypes');
            setAvailableServices(response.data);
            setOpenSearch(true);
        } catch (e) {
            triggerNotification('FAIL', 'Error fetching Available Services', `${e.message}`);
        } finally {
            hideLoading();
        }
    };

    const setFetchUserRequestsCallback = (callback) => {
        setFetchUserRequests(() => callback);
    };

    return (
        <RaiseRequestContext.Provider value={{ showSearch, setFetchUserRequestsCallback, handleServiceTypeSelect }}>
            {children}

            {/* Service Search Dialog */}
            <Dialog className="relative z-40" open={openSearch} onClose={handleRequestSearchOnClose}>
                <DialogBackdrop
                    transition
                    className="fixed inset-0 bg-gray-500 bg-opacity-25 transition-opacity data-[closed]:opacity-0 data-[enter]:duration-300 data-[leave]:duration-200 data-[enter]:ease-out data-[leave]:ease-in"
                />
                <div className="fixed inset-0 z-10 w-screen overflow-y-auto p-4 sm:p-6 md:p-20">
                    <DialogPanel
                        transition
                        className="mx-auto max-w-2xl transform divide-y divide-gray-100 overflow-hidden rounded-xl bg-white shadow-2xl ring-1 ring-black ring-opacity-5 transition-all data-[closed]:scale-95 data-[closed]:opacity-0 data-[enter]:duration-300 data-[leave]:duration-200 data-[enter]:ease-out data-[leave]:ease-in"
                    >
                        <Combobox>
                            <div className="relative">
                                <MagnifyingGlassIcon className="pointer-events-none absolute left-4 top-3.5 h-5 w-5 text-gray-400" aria-hidden="true" />
                                <ComboboxInput
                                    autoFocus
                                    className="h-12 w-full border-0 bg-transparent pl-11 pr-4 text-gray-900 placeholder:text-gray-400 focus:ring-0 sm:text-sm"
                                    placeholder="Search"
                                    onChange={(event) => setQuery(event.target.value)}
                                    onBlur={() => setQuery('')}
                                />
                            </div>
                            <AnimatePresence>
                                {filteredServices && filteredServices.length > 0 && (
                                    <ComboboxOptions static as="div" className="max-h-80 scroll-py-2 divide-y divide-gray-100 overflow-y-auto">
                                        <h2 className="px-4 py-2.5 text-base font-semibold text-gray-900">Available Services</h2>
                                        {filteredServices.map((item) => (
                                            <motion.div
                                                key={item.id}
                                                initial={{ opacity: 0, x: -20 }}
                                                animate={{ opacity: 1, x: 0 }}
                                                exit={{ opacity: 0, x: -20 }}
                                                transition={{ duration: 0.2 }}
                                            >
                                                <ComboboxOption
                                                    as="li"
                                                    onClick={() => {
                                                        handleServiceTypeSelect(item);
                                                        setOpenSearch(false);
                                                        setQuery('');
                                                    }}
                                                    key={item.id}
                                                    value={item.id}
                                                    className="group flex items-center cursor-default select-none rounded-xl p-3 data-[focus]:bg-gray-100 hover:cursor-pointer"
                                                >
                                                    <div className='flex h-10 w-10 flex-none items-center justify-center rounded-lg'>
                                                        <img className="h-10 w-10" aria-hidden="true" src={item.serviceIcon} alt={item.name} />
                                                    </div>
                                                    <div className="ml-4 flex-auto">
                                                        <p className="text-sm font-medium text-gray-700 group-data-[focus]:text-gray-900">{item.name}</p>
                                                        <p className="text-sm text-gray-500 group-data-[focus]:text-gray-700">{item.description}</p>
                                                    </div>
                                                    <div className="flex items-center gap-2">
                                                        {item.isPaid
                                                            ? <span className="text-sm font-semibold text-indigo-600">£{item.price?.toFixed(2)}</span>
                                                            : <span className="text-sm text-gray-400">Free</span>
                                                        }
                                                        <ChevronRightIcon className="ml-3 h-5 w-5 flex-none text-gray-400" aria-hidden="true" />
                                                    </div>
                                                </ComboboxOption>
                                            </motion.div>
                                        ))}
                                    </ComboboxOptions>
                                )}
                                {query !== '' && filteredServices.length === 0 && (
                                    <div className="px-6 py-14 text-center text-sm sm:px-14">
                                        <ExclamationCircleIcon className="mx-auto h-6 w-6 text-gray-400" />
                                        <p className="mt-4 font-semibold text-gray-900">No results found</p>
                                        <p className="mt-2 text-gray-500">No Services found for this search term. Please try again.</p>
                                    </div>
                                )}
                            </AnimatePresence>
                        </Combobox>
                    </DialogPanel>
                </div>
            </Dialog>

            {/* Raise Request Dialog */}
            <Dialog open={openRequestRaiseDialog} onClose={() => { return }} className="relative z-40">
                <DialogBackdrop
                    transition
                    className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity data-[closed]:opacity-0 data-[enter]:duration-300 data-[leave]:duration-200 data-[enter]:ease-out data-[leave]:ease-in"
                />
                <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
                    <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
                        <DialogPanel
                            transition
                            className="relative transform rounded-lg bg-zinc-100 px-4 pb-4 pt-5 text-left shadow-xl transition-all data-[closed]:translate-y-4 data-[closed]:opacity-0 data-[enter]:duration-300 data-[leave]:duration-200 data-[enter]:ease-out data-[leave]:ease-in sm:my-8 sm:max-w-lg sm:p-6 data-[closed]:sm:translate-y-0 data-[closed]:sm:scale-95 w-full max-w-full"
                        >
                            <motion.div layout transition={{ duration: 0.2, ease: "easeInOut" }}>
                                <div className="text-center">
                                    <DialogTitle as="h3" className="flex justify-between text-base font-semibold leading-6 text-gray-900 mb-4">
                                        <div className="flex gap-2">
                                            <TicketIcon className="text-indigo-600 h-6 w-6" />
                                            <h2>{step === 'payment' ? 'Payment' : 'Raise Request'}</h2>
                                        </div>
                                        {step === 'form' && selectedService?.isPaid && (
                                            <span className="text-indigo-600 font-semibold">£{selectedService?.price?.toFixed(2)}</span>
                                        )}
                                    </DialogTitle>

                                    <AnimatePresence mode="wait">
                                        {step === 'form' ? (
                                            <motion.div
                                                key="form"
                                                initial={{ opacity: 0, y: 20 }}
                                                animate={{ opacity: 1, y: 0 }}
                                                exit={{ opacity: 0, y: -20 }}
                                                transition={{ duration: 0.1 }}
                                            >
                                                <Formik
                                                    initialValues={initialValues}
                                                    enableReinitialize
                                                    validationSchema={ServiceRequestSchema}
                                                    onSubmit={handleSubmit}
                                                >
                                                    {({ errors, touched, isSubmitting, isValid, dirty, setFieldValue }) => (
                                                        <Form className="grid grid-cols-1 gap-y-6 text-left">
                                                            <div className="col-span-full">
                                                                <div className={cn(defaultInputBorder, 'flex flex-row justify-between items-center')}>
                                                                    <div className="flex flex-row">
                                                                        <img src={selectedService?.serviceIcon} className="h-20 w-20 p-4" alt={selectedService?.name} />
                                                                        <div className="mt-4 mb-4 sm:m-4 flex flex-col">
                                                                            <p className="text-sm font-medium text-gray-700">{selectedService?.name}</p>
                                                                            <p className="text-sm text-gray-500">{selectedService?.description}</p>
                                                                        </div>
                                                                    </div>
                                                                    {selectedService?.isPaid
                                                                        ? <span className="mr-4 text-sm font-semibold text-indigo-600">£{selectedService?.price?.toFixed(2)}</span>
                                                                        : <span className="mr-4 text-sm text-gray-400">Free</span>
                                                                    }
                                                                </div>
                                                            </div>

                                                            {user.buildings && user.buildings.length > 0 && (
                                                                <div className="col-span-full">
                                                                    <div className="flex justify-between">
                                                                        <label className="block text-sm font-medium leading-6 text-gray-900">Where is the issue located?</label>
                                                                        <span className="text-sm leading-6 text-gray-500">Required</span>
                                                                    </div>
                                                                    <Field name="buildingId">
                                                                        {({ field }) => (
                                                                            <Listbox value={field.value} onChange={(value) => setFieldValue('buildingId', value)}>
                                                                                <div className="relative mt-2 w-full">
                                                                                    <ListboxButton className={touched.buildingId && errors.buildingId ? errorBorder : defaultInputBorder}>
                                                                                        <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-2">
                                                                                            <MapPinIcon aria-hidden="true" className="h-5 w-5 text-gray-400" />
                                                                                        </span>
                                                                                        <span className={field.value === '' ? "block truncate opacity-80" : "block truncate"}>
                                                                                            {field.value ? user.buildings.find(p => p.id === field.value)?.name : '-Select Building-'}
                                                                                        </span>
                                                                                        <span className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2">
                                                                                            <ChevronDownIcon aria-hidden="true" className="h-5 w-5 text-gray-400" />
                                                                                        </span>
                                                                                    </ListboxButton>
                                                                                    <ListboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                                                                                        {user.buildings.map((option) => (
                                                                                            <ListboxOption key={option.id} value={option.id} className="flex gap-4 relative cursor-default select-none py-2 pl-3 pr-9 text-gray-900">
                                                                                                <span className="block truncate font-normal">{option.name}</span>
                                                                                                {field.value === option.id && (
                                                                                                    <span className="absolute inset-y-0 right-0 flex items-center pr-4 text-indigo-600">
                                                                                                        <CheckIcon aria-hidden="true" className="h-5 w-5" />
                                                                                                    </span>
                                                                                                )}
                                                                                            </ListboxOption>
                                                                                        ))}
                                                                                    </ListboxOptions>
                                                                                </div>
                                                                            </Listbox>
                                                                        )}
                                                                    </Field>
                                                                </div>
                                                            )}

                                                            <div className="col-span-full">
                                                                <div className="flex justify-between">
                                                                    <label className="block text-sm font-medium leading-6 text-gray-900">What's the priority of your request?</label>
                                                                    <span className="text-sm leading-6 text-gray-500">Required</span>
                                                                </div>
                                                                <Field name="priority">
                                                                    {({ field }) => (
                                                                        <Listbox value={field.value} onChange={(value) => setFieldValue('priority', value)}>
                                                                            <div className="relative mt-2 w-full">
                                                                                <ListboxButton className={touched.priority && errors.priority ? errorBorder : defaultInputBorder}>
                                                                                    <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-2">
                                                                                        <ExclamationTriangleIcon aria-hidden="true" className="h-5 w-5 text-gray-400" />
                                                                                    </span>
                                                                                    <span className={field.value === '' ? "block truncate opacity-80" : "block truncate"}>
                                                                                        {field.value ? priorityOptions.find(p => p.id === field.value)?.name : '-Select Priority-'}
                                                                                    </span>
                                                                                    <span className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2">
                                                                                        <ChevronDownIcon aria-hidden="true" className="h-5 w-5 text-gray-400" />
                                                                                    </span>
                                                                                </ListboxButton>
                                                                                <ListboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                                                                                    {priorityOptions.map((option) => (
                                                                                        <ListboxOption key={option.id} value={option.id} className="flex gap-4 relative cursor-default select-none py-2 pl-3 pr-9 text-gray-900">
                                                                                            <span className="block truncate font-normal">{option.name}</span>
                                                                                            {field.value === option.id && (
                                                                                                <span className="absolute inset-y-0 right-0 flex items-center pr-4 text-indigo-600">
                                                                                                    <CheckIcon aria-hidden="true" className="h-5 w-5" />
                                                                                                </span>
                                                                                            )}
                                                                                        </ListboxOption>
                                                                                    ))}
                                                                                </ListboxOptions>
                                                                            </div>
                                                                        </Listbox>
                                                                    )}
                                                                </Field>
                                                                <ErrorMessage name="priority" component="div" className="text-red-600 text-sm mt-1" />
                                                            </div>

                                                            <div className="col-span-full">
                                                                <div className="flex justify-between">
                                                                    <label className="block text-sm font-medium leading-6 text-gray-900">Is there anything we need to know before starting?</label>
                                                                    <span className="text-sm leading-6 text-gray-500">Optional</span>
                                                                </div>
                                                                <div className="mt-2">
                                                                    <Field
                                                                        name="customerNotes"
                                                                        id="customerNotes"
                                                                        as="textarea"
                                                                        rows="4"
                                                                        placeholder="Enter your requirements here..."
                                                                        className={errors.customerNotes && touched.customerNotes ? errorBorder : defaultInputBorder}
                                                                    />
                                                                    <ErrorMessage name="customerNotes" component="div" className="mt-2 text-sm text-red-600" />
                                                                </div>
                                                            </div>

                                                            <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                                                                <button
                                                                    type="submit"
                                                                    className={cn(
                                                                        "inline-flex w-full justify-center rounded-md px-3 py-2 text-sm font-semibold shadow-sm sm:col-start-2",
                                                                        isValid && !isSubmitting
                                                                            ? "bg-indigo-600 text-white hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                                                                            : "bg-indigo-400 text-gray-200 cursor-not-allowed"
                                                                    )}
                                                                    disabled={!isValid || isSubmitting}
                                                                >
                                                                    {isSubmitting ? 'Loading...' : selectedService?.isPaid ? 'Next' : 'Submit'}
                                                                </button>
                                                                <button
                                                                    type="button"
                                                                    onClick={handleRaiseRequestDialogOnClose}
                                                                    className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:col-start-1 sm:mt-0"
                                                                >
                                                                    Cancel
                                                                </button>
                                                            </div>
                                                        </Form>
                                                    )}
                                                </Formik>
                                            </motion.div>
                                        ) : (
                                            <motion.div
                                                key="payment"
                                                initial={{ opacity: 0, x: 20 }}
                                                animate={{ opacity: 1, x: 0 }}
                                                exit={{ opacity: 0, x: 20 }}
                                                transition={{ duration: 0.2 }}
                                            >
                                                <Elements stripe={stripePromise} options={{ clientSecret }}>
                                                    <PaymentForm
                                                        clientSecret={clientSecret}
                                                        selectedService={selectedService}
                                                        onSuccess={handlePaymentSuccess}
                                                        onBack={handlePaymentBack}
                                                    />
                                                </Elements>
                                            </motion.div>
                                        )}
                                    </AnimatePresence>
                                </div>
                            </motion.div>
                        </DialogPanel>
                    </div>
                </div>
            </Dialog>
        </RaiseRequestContext.Provider>
    );
};