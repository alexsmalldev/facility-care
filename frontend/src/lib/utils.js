import { clsx } from "clsx";
import { twMerge } from "tailwind-merge"

export function cn(...inputs) {
  return twMerge(clsx(inputs));
}

export function decodeToken(token) {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return {
        id: payload.sub,
        username: payload.unique_name,
        email: payload.email,
        first_name: payload.first_name,
        last_name: payload.last_name,
        user_type: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']?.toLowerCase()
    };
};