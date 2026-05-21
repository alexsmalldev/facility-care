import React, { createContext, useState, useEffect, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { setToken, removeToken, getAccessToken } from '../api/tokenVerification';
import { useWebSocket } from '../hooks/data/useWebSocket';
import { useLoading } from '../contexts/LoadingContext';
import { decodeToken } from '../lib/utils';

const UserContext = createContext();

export const UserProvider = ({ children }) => {
    const { showLoading, hideLoading } = useLoading();
    const [user, setUser] = useState(null);
    const [selectedBuilding, setSelectedBuilding] = useState(null);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    const hasRole = (role) => user?.user_type === role;

    useEffect(() => {
        const restoreSession = async () => {
            try {
                // attempt to get a new access token using the refresh cookie
                const response = await axios.post(
                    `${import.meta.env.VITE_API_URL}/auth/refresh-token`,
                    {},
                    { withCredentials: true }
                );

                const { access } = response.data;
                setToken(access);

                const decoded = decodeToken(access);
                setUser(decoded);
            } catch {
                // no valid refresh cookie - user needs to login
                removeToken();
                setUser(null);
            } finally {
                setLoading(false);
            }
        };

        restoreSession();
    }, []);

    useEffect(() => {
        const handleTokenExpiration = async () => {
            try {
                showLoading('Logging out...');
                await axios.post(
                    `${import.meta.env.VITE_API_URL}/auth/logout`,
                    {},
                    { withCredentials: true }
                );
            } catch {
                // ignore errors on logout
            } finally {
                removeToken();
                setUser(null);
                hideLoading();
                navigate('/login');
            }
        };

        window.addEventListener('tokenExpired', handleTokenExpiration);
        return () => window.removeEventListener('tokenExpired', handleTokenExpiration);
    }, [navigate]);

    return (
        <UserContext.Provider value={{ user, setUser, hasRole, loading, selectedBuilding, setSelectedBuilding }}>
            {children}
        </UserContext.Provider>
    );
};

export const useUser = () => useContext(UserContext);