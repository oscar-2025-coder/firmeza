import { useState, useEffect } from "react";
import { fetchProducts } from "../api/products";
import { useCart } from "../context/CartContext";
import Navbar from "../components/Navbar";

// IMPORTAR ALERTA CENTRADA
import { showCenterAlert } from "../utils/alerts";

export default function ProductsPage() {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const { addToCart } = useCart();

    useEffect(() => {
        loadProducts();
    }, []);

    const loadProducts = async () => {
        try {
            const data = await fetchProducts();
            setProducts(data);
        } catch (err) {
            setError("Error al cargar productos");
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleAddToCart = (product) => {
        addToCart(product, 1);

        // ALERTA MODERNA, CENTRADA, ELEGANTE
        showCenterAlert(`${product.name} agregado al carrito`);
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-[#0D0D0D] flex items-center justify-center">
                <p className="text-white text-xl animate-pulse">Cargando productos...</p>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-[#0D0D0D] text-white">

            <Navbar />

            <main className="max-w-7xl mx-auto py-12 px-6">
                <h2 className="text-3xl font-bold mb-8">Catálogo de Productos</h2>

                {error && (
                    <p className="text-red-500 mb-4">{error}</p>
                )}

                {products.length === 0 ? (
                    <p className="text-gray-400">No hay productos disponibles.</p>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                        {products.map(product => (
                            <div
                                key={product.id}
                                className="
                                    bg-[#1A1A1A] 
                                    rounded-xl 
                                    p-6 
                                    shadow-lg 
                                    hover:shadow-2xl 
                                    hover:scale-[1.03]
                                    transition-all 
                                    border border-gray-800
                                "
                            >
                                {/* Título */}
                                <h3 className="text-xl font-semibold mb-2 text-white">
                                    {product.name}
                                </h3>

                                {/* SKU */}
                                <p className="text-gray-400 text-sm mb-4">
                                    SKU: {product.sku ?? "N/A"}
                                </p>

                                {/* Precio y Stock */}
                                <div className="mb-4">
                                    <p className="text-2xl font-bold text-blue-400 drop-shadow-sm">
                                        ${product.unitPrice.toLocaleString()}
                                    </p>
                                    <p className="text-gray-500 text-sm">
                                        Stock: {product.stock} unidades
                                    </p>
                                </div>

                                {/* Botón */}
                                {product.isActive && product.stock > 0 ? (
                                    <button
                                        onClick={() => handleAddToCart(product)}
                                        className="
                                            w-full py-2 
                                            bg-green-600 
                                            hover:bg-green-700 
                                            rounded-lg 
                                            font-semibold 
                                            text-white
                                            shadow-md
                                            hover:shadow-xl
                                            transition
                                        "
                                    >
                                        Agregar al Carrito
                                    </button>
                                ) : (
                                    <button
                                        disabled
                                        className="
                                            w-full py-2 
                                            bg-gray-600 
                                            rounded-lg 
                                            font-semibold 
                                            cursor-not-allowed
                                            text-gray-300
                                        "
                                    >
                                        No Disponible
                                    </button>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </main>
        </div>
    );
}
