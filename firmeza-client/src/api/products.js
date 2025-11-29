import api from "./axios";

// GET ALL PRODUCTS
export async function fetchProducts() {
    const response = await api.get("/Products");
    return response.data;
}

// GET PRODUCT BY ID
export async function fetchProductById(id) {
    const response = await api.get(`/Products/${id}`);
    return response.data;
}
