import React from 'react';
import { ChevronRightIcon } from '@heroicons/react/20/solid';
import moment from 'moment';
import { Card, CardTitle, CardContent, CardHeader, CardDescription, CardFooter } from '../../../../components/ui/card';
import { CalendarCheck, Calendar, TriangleAlert, Pencil, CreditCard } from 'lucide-react';

const RequestTile = ({ request }) => {

    const formatDate = (value) => {
        return moment(value).format('MMMM Do YYYY, h:mm:ss A');
    }

    const paymentChip = () => {
        if (!request.serviceType?.isPaid) return null;

        const chipClass = request.paymentStatus === 'Paid'
            ? 'bg-green-50 text-green-700 ring-green-600/20'
            : request.paymentStatus === 'Refunded'
                ? 'bg-orange-50 text-orange-700 ring-orange-600/20'
                : 'bg-yellow-50 text-yellow-700 ring-yellow-600/20';

        return (
            <span className={`inline-flex items-center gap-1 rounded-md px-2 py-1 text-xs font-medium ring-1 ring-inset ${chipClass}`}>
                <CreditCard className="h-3 w-3" />
                {request.paymentStatus}
            </span>
        );
    };

    const statusChip = () => {
        const isOverdue = new Date() > new Date(request.serviceLevelAgreementDate) &&
            request.status !== 'Completed' && request.status !== 'Cancelled';

        const statusClass = () => {
            switch (request.status) {
                case 'Completed':
                    return 'bg-green-50 text-green-700 ring-green-600/20';
                case 'Cancelled':
                    return 'bg-orange-50 text-orange-700 ring-orange-600/20';
                default:
                    return isOverdue ? 'bg-red-50 text-red-700 ring-red-600/20' : 'bg-white text-gray-700 ring-gray-600/20';
            }
        };

        const formatStatus = (status) => {
            switch (status) {
                case 'InProgress': return 'In Progress';
                default: return status;
            }
        };

        return (
            <div className="flex shrink-0 items-center gap-x-4">
                <span className={`inline-flex items-center rounded-md mx-2 px-2 py-1 text-xs font-medium ring-1 ring-inset ${statusClass()}`}>
                    {isOverdue ? (
                        <>
                            OVERDUE <br />
                            {formatStatus(request.status)}
                        </>
                    ) : (
                        formatStatus(request.status)
                    )}
                </span>
            </div>
        );
    };

    return (
        <>
            <Card className="flex-1 hover:bg-gray-50 hover:cursor-pointer flex flex-row justify-between items-center">
                <div>
                    <CardHeader>
                        <CardTitle>Request: {request.id}</CardTitle>
                        <CardDescription>{request.serviceType?.name} - {request.building?.name}</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <div className="flex flex-row justify-between">
                            <div className="flex flex-row items-center justify-start gap-4">
                                <img
                                    src={request.serviceType?.serviceIcon}
                                    alt={request.serviceType?.name}
                                    className="h-14 w-14" />
                                <div className="min-w-0 flex-auto">
                                    <p className="m-2 text-xs text-gray-500 flex flex-row justify-start gap-2 items-center">
                                        <Pencil className="h-4 w-4" />
                                        {request.customerNotes !== '' ? `Notes: ${request.customerNotes}` : "No additional notes provided."}
                                    </p>
                                    <p className="m-2 text-xs text-gray-500 flex flex-row justify-start gap-2 items-center">
                                        <Calendar className="h-4 w-4" />
                                        {`Created Date: ${formatDate(request.createdDate)}`}
                                    </p>
                                    <p className="m-2 text-xs text-gray-500 flex flex-row justify-start gap-2 items-center">
                                        <CalendarCheck className="h-4 w-4" />
                                        {`SLA Date: ${formatDate(request.serviceLevelAgreementDate)}`}
                                    </p>
                                    <p className="m-2 text-xs text-gray-500 flex flex-row justify-start gap-2 items-center">
                                        <TriangleAlert className="h-4 w-4" />
                                        {`Priority: ${request.priority}`}
                                    </p>
                                    {request.serviceType?.isPaid && (
                                        <p className="m-2 text-xs text-gray-500 flex flex-row justify-start gap-2 items-center">
                                            <CreditCard className="h-4 w-4" />
                                            {paymentChip()}
                                            {request.serviceType?.price && `£${request.serviceType.price.toFixed(2)}`}
                                        </p>
                                    )}
                                </div>
                            </div>
                        </div>
                    </CardContent>
                    <CardFooter className="lg:hidden">
                        <div className="flex gap-2">
                            {paymentChip()}
                            {statusChip()}
                        </div>
                    </CardFooter>
                </div>
                <div className="hidden lg:flex flex-row items-center gap-2 mr-4">
                    {paymentChip()}
                    {statusChip()}
                    <ChevronRightIcon aria-hidden="true" className="h-6 w-6 flex-none text-gray-400" />
                </div>
            </Card>
        </>
    );
};

export default RequestTile;