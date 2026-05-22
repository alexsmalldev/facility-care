import { useState, useEffect } from 'react';
import api from '../../api/apiConfig';
import { useNotification } from '../../contexts/NotificationContext';
import { useLoading } from '../../contexts/LoadingContext';

export const useMyRequests = () => {
    const { showLoading, hideLoading } = useLoading();
    const { triggerNotification } = useNotification();

    const [userRequests, setUserRequests] = useState([]);
    const [filters, setFilters] = useState({
        buildingId: null,
        serviceTypeId: null,
        priority: null,
        status: null,
    });

    const fetchUserRequests = async () => {
        showLoading('Fetching Requests');
        const queryParams = new URLSearchParams();

        for (const [key, value] of Object.entries(filters)) {
            if (value) {
                queryParams.append(key, value.id || value);
            }
        }

        const endpoint = `/ServiceRequests${queryParams.toString() ? `?${queryParams.toString()}` : ''}`;

        try {
            const response = await api.get(endpoint);
            setUserRequests(response.data);
        } catch (e) {
            triggerNotification('FAIL', 'Error fetching your requests', `${e.message}`);
        } finally {
            hideLoading();
        }
    };

    useEffect(() => {
        fetchUserRequests();
    }, [filters]);

    const updateFilter = (filterName, value) => {
        setFilters(prev => ({ ...prev, [filterName]: value }));
    };

    return {
        userRequests,
        filters,
        updateFilter,
        fetchUserRequests,
    };
};