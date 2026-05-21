let accessToken = null;

export const setToken = (token) => {
    accessToken = token;
};

export const getAccessToken = () => {
    return accessToken;
};

export const removeToken = () => {
    accessToken = null;
};