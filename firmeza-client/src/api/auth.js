import api from "./axios";

// LOGIN CLIENTE
export async function loginUser(credentials) {
    const response = await api.post("/Auth/login", {
        email: credentials.email,
        password: credentials.password,
    });

    return response.data; // { token, expiration }
}

// REGISTER CLIENTE
export async function registerUser(data) {
    const response = await api.post("/Auth/register", {
        fullName: data.fullName,
        documentNumber: data.documentNumber,
        email: data.email,
        phoneNumber: data.phoneNumber,
        age: Number(data.age),
        password: data.password,
    });

    return response.data;
}
