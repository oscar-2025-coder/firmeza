import api from "./axios";

// CREATE SALE
export async function createSale(customerId, items, notes = "") {
    const response = await api.post("/Sales", {
        customerId,
        items,
        notes
    });
    return response.data;
}

// GET ALL SALES
export async function fetchSales() {
    const response = await api.get("/Sales");
    return response.data;
}

// GET SALE BY ID
export async function fetchSaleById(id) {
    const response = await api.get(`/Sales/${id}`);
    return response.data;
}
