import axios from 'axios';
import { useAuth } from './../context/Authcontext';

const api = axios.create({ baseURL: 'http://localhost:5149/api' });

api.interceptors.request.use((config) => {
    const token = useAuth
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
});

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        if (error.response?.status === 401) {
            const res = await axios.post('/Usuario/RefreshToken', {}, { withCredentials: true });
            if (res.data.accessToken) {
            }
        }
        return Promise.reject(error);
    }
);