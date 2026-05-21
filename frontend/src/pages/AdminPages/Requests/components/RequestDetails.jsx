import React from "react";
import { Link } from "react-router-dom";
import moment from 'moment';
import { useUser } from "../../../../contexts/UserContext";

const RequestDetails = ({ requestDetails }) => {
    const { hasRole } = useUser();

    const formatDate = (value) => {
        return moment(value).format('MMMM Do YYYY, h:mm:ss A');
    }

    return (
        <div className="relative flex-1 h-full overflow-y-auto">
            <div className="py-10">
                <div className="border-b border-gray-200 pb-5 sm:flex sm:items-center sm:justify-between">
                    <div>
                        <h3 className="text-base font-semibold leading-7 text-gray-900">Details</h3>
                        <p className="mt-1 max-w-2xl text-sm leading-6 text-gray-400">Information regarding the Request provided by the Customer.</p>
                    </div>
                </div>

                <div className="mt-4">
                    <dl className="divide-y divide-gray-100">
                        <div className="py-4 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0 text-sm">
                            <dt className="font-medium leading-6 text-gray-500">Service</dt>
                            <dd className="mt-1 leading-6 text-gray-900 sm:col-span-2 sm:mt-0 font-medium break-words whitespace-normal">
                                {requestDetails?.serviceType?.name}
                            </dd>
                        </div>
                        <div className="py-4 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0 text-sm">
                            <dt className="font-medium leading-6 text-gray-500">Description</dt>
                            <dd className="mt-1 leading-6 text-gray-900 sm:col-span-2 sm:mt-0 font-medium break-words whitespace-normal">
                                {requestDetails?.serviceType?.description}
                            </dd>
                        </div>
                        <div className="py-4 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0 text-sm">
                            <dt className="font-medium leading-6 text-gray-500">Customer Notes</dt>
                            <dd className="mt-1 leading-6 text-gray-900 sm:col-span-2 sm:mt-0 font-medium break-words whitespace-normal">
                                {requestDetails?.customerNotes}
                            </dd>
                        </div>
                        <div className="py-4 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0 text-sm">
                            <dt className="font-medium leading-6 text-gray-500">Location</dt>
                            {hasRole('admin') ?
                                <dd className="mt-1 leading-6 text-indigo-600 sm:col-span-2 sm:mt-0 font-medium break-words whitespace-normal">
                                    <Link to={`/buildings/${requestDetails?.building?.id}`}>
                                        {requestDetails?.building?.name}
                                    </Link>
                                </dd>
                                :
                                <dd className="mt-1 leading-6 text-gray-900 sm:col-span-2 sm:mt-0 font-medium break-words whitespace-normal">
                                    {requestDetails?.building?.name}
                                </dd>
                            }
                        </div>
                        <div className="py-4 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0 text-sm">
                            <dt className="font-medium leading-6 text-gray-500">Priority</dt>
                            <dd className="mt-1 leading-6 text-gray-900 sm:col-span-2 sm:mt-0 font-medium">
                                {requestDetails?.priority}
                            </dd>
                        </div>
                        <div className="py-4 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0 text-sm">
                            <dt className="font-medium leading-6 text-gray-500">Created By</dt>
                            <dd className="mt-1 leading-6 text-gray-900 sm:col-span-2 sm:mt-0 font-medium">
                                {requestDetails?.createdByFirstName + ' ' + requestDetails?.createdByLastName}
                            </dd>
                        </div>
                        <div className="py-4 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0 text-sm">
                            <dt className="font-medium leading-6 text-gray-500">Create Date</dt>
                            <dd className="mt-1 leading-6 text-gray-900 sm:col-span-2 sm:mt-0 font-medium">
                                {formatDate(requestDetails?.createdDate)}
                            </dd>
                        </div>
                        <div className="py-4 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0 text-sm">
                            <dt className="font-medium leading-6 text-gray-500">Target Completion Date</dt>
                            <dd className="mt-1 leading-6 text-gray-900 sm:col-span-2 sm:mt-0 font-medium">
                                {formatDate(requestDetails?.serviceLevelAgreementDate)}
                            </dd>
                        </div>
                    </dl>
                </div>
            </div>
        </div>
    );
}

export default RequestDetails;