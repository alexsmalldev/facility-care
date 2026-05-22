import { useNavigate } from 'react-router-dom';
import api from '../../api/apiConfig';
import { useLoading } from '../../contexts/LoadingContext';
import { setToken, removeToken } from '../../api/tokenVerification';
import { useUser } from '../../contexts/UserContext';
import { useWebSocket } from './useWebSocket';
import { decodeToken } from '../../lib/utils';

export const useAuth = () => {
    const { showLoading, hideLoading } = useLoading();
    const navigate = useNavigate();
    const { setUser } = useUser();

    const login = async (credentials) => {
        debugger;
        try {
            showLoading('Logging in...');
            const response = await api.post('/auth/login', credentials);
            const { access } = response.data;

            setToken(access);
            const user = decodeToken(access);
            if (user.user_type === 'regular') {
                const buildingsResponse = await api.get('/Buildings');
                setUser({ ...user, buildings: buildingsResponse.data });
            } else {
                setUser(user);
            }
            navigate('/');
        } catch (error) {
            removeToken();
            throw error;
        } finally {
            hideLoading();
        }
    };

    const logout = async () => {
        try {
            showLoading('Logging out...');
            await api.post('/auth/logout');
        } catch (error) {
            throw error;
        } finally {
            removeToken();
            setUser(null);
            hideLoading();
            navigate('/login');
        }
    };

    const register = async (userData) => {
        try {
            showLoading('Registering...');
            const response = await api.post('/auth/register', userData);
            return response.data;
        } catch (error) {
            throw error;
        } finally {
            hideLoading();
        }
    };

    return { login, logout, register };
};