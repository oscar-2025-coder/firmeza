import { createContext, useState, useEffect } from "react";
import { jwtDecode } from "jwt-decode";
import { loginUser } from "../api/auth";

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {

    const [token, setToken] = useState(localStorage.getItem("token"));
    const [user, setUser] = useState(null);

    // Cargar usuario desde token al iniciar la app
    useEffect(() => {
        if (token) {
            try {
                const decoded = jwtDecode(token);

                // Validar expiración
                if (decoded.exp * 1000 < Date.now()) {
                    logout();
                } else {
                    setUser({ email: decoded.email, ...decoded });
                }

            } catch (err) {
                console.error("Invalid token:", err);
                logout();
            }
        }
    }, [token]);

    // LOGIN REAL
    const login = async (email, password) => {
        try {
            const data = await loginUser({ email, password });
            const tkn = data.token;

            localStorage.setItem("token", tkn);
            setToken(tkn);

            // Decodificar para actualizar estado inmediatamente
            const decoded = jwtDecode(tkn);
            setUser({ email: decoded.email, ...decoded });

            return true;
        } catch (error) {
            console.error("Login failed:", error);
            throw error;
        }
    };

    // LOGOUT
    const logout = () => {
        localStorage.removeItem("token");
        setToken(null);
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ token, user, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};
