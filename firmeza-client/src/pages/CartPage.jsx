import { useState, useContext } from "react";
import { useCart } from "../context/CartContext";
import { AuthContext } from "../context/AuthContext";
import { createSale } from "../api/sales";
import { useNavigate } from "react-router-dom";
import { jwtDecode } from "jwt-decode";
import Navbar from "../components/Navbar";

// Alertas modernas
import { showError, showSuccess } from "../utils/alerts";

export default function CartPage() {
    const { cartItems, updateQuantity, removeFromCart, clearCart, subtotal, tax, total } = useCart();
    const { token } = useContext(AuthContext);
    const navigate = useNavigate();

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState(false);

    const handleCheckout = async () => {
        if (cartItems.length === 0) {
            showError("El carrito está vacío");
            return;
        }

        if (!token) {
            showError("Debes iniciar sesión para comprar");
            navigate("/login");
            return;
        }

        setLoading(true);
        setError("");

        try {
            // ----------------------------
            // ✅ OBTENER customerId CORRECTAMENTE
            // ----------------------------
            const decoded = jwtDecode(token);
            const customerId = decoded.customerId; // ✔ Claim correcto

            if (!customerId) {
                showError("No se pudo validar tu cuenta. Inicia sesión nuevamente.");
                return;
            }

            const items = cartItems.map(item => ({
                productId: item.productId,
                quantity: item.quantity
            }));

            await createSale(customerId, items, "Compra desde frontend");

            clearCart();
            showSuccess("Compra realizada con éxito. Recibirás tu comprobante por correo.");
            setSuccess(true);

            setTimeout(() => {
                navigate("/products");
            }, 2500);

        } catch (err) {
            console.error("Error al procesar la compra:", err);
            const message = err.response?.data || "Error al procesar la compra. Intenta nuevamente.";
            setError(message);
            showError(message);
        } finally {
            setLoading(false);
        }
    };

    if (success) {
        return (
            <div className="min-h-screen bg-[#0D0D0D] flex items-center justify-center">
                <div className="bg-green-600 text-white p-8 rounded-2xl shadow-2xl text-center max-w-md">
                    <h2 className="text-3xl font-bold mb-4">¡Compra Exitosa!</h2>
                    <p className="text-lg mb-4">
                        Tu compra se ha procesado correctamente.
                    </p>
                    <p className="text-sm">
                        Recibirás un correo con tu comprobante de compra.
                    </p>
                    <p className="text-gray-200 mt-4">Redirigiendo...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-[#0D0D0D] text-white">

            <Navbar />

            <main className="max-w-4xl mx-auto py-12 px-6">
                <h2 className="text-3xl font-bold mb-8">Carrito de Compras 🛒</h2>

                {error && (
                    <div className="bg-red-600 text-white p-4 rounded-lg mb-6">
                        {error}
                    </div>
                )}

                {cartItems.length === 0 ? (
                    <div className="bg-[#1A1A1A] rounded-xl p-10 text-center">
                        <p className="text-gray-300 text-xl mb-4">
                            Tu carrito está vacío
                        </p>
                        <button
                            onClick={() => navigate("/products")}
                            className="px-6 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg font-semibold transition"
                        >
                            Ver Productos
                        </button>
                    </div>
                ) : (
                    <div className="bg-[#1A1A1A] rounded-xl p-8 shadow-lg">

                        <div className="space-y-4 mb-6">
                            {cartItems.map(item => (
                                <div
                                    key={item.productId}
                                    className="flex justify-between items-center border-b border-gray-700 pb-4"
                                >
                                    <div className="flex-1">
                                        <h3 className="text-lg font-semibold">{item.productName}</h3>
                                        <p className="text-gray-400">
                                            ${item.unitPrice.toLocaleString()} x {item.quantity}
                                        </p>
                                    </div>

                                    <div className="flex items-center gap-4">
                                        <div className="flex items-center gap-2">
                                            <button
                                                onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                                                className="px-3 py-1 bg-gray-700 hover:bg-gray-600 rounded-lg"
                                            >
                                                -
                                            </button>
                                            <span className="w-12 text-center">{item.quantity}</span>
                                            <button
                                                onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                                                className="px-3 py-1 bg-gray-700 hover:bg-gray-600 rounded-lg"
                                            >
                                                +
                                            </button>
                                        </div>

                                        <div className="w-32 text-right font-bold text-blue-400">
                                            ${(item.unitPrice * item.quantity).toLocaleString()}
                                        </div>

                                        <button
                                            onClick={() => removeFromCart(item.productId)}
                                            className="px-3 py-1 bg-red-600 hover:bg-red-700 rounded-lg"
                                        >
                                            ✕
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div className="border-t border-gray-700 pt-6 space-y-2">
                            <div className="flex justify-between text-lg">
                                <span>Subtotal:</span>
                                <span>${subtotal.toLocaleString()}</span>
                            </div>
                            <div className="flex justify-between text-lg">
                                <span>IVA (19%):</span>
                                <span>${tax.toLocaleString()}</span>
                            </div>
                            <div className="flex justify-between text-2xl font-bold">
                                <span>Total:</span>
                                <span className="text-green-400">${total.toLocaleString()}</span>
                            </div>
                        </div>

                        <button
                            onClick={handleCheckout}
                            disabled={loading}
                            className="w-full mt-8 py-4 bg-green-600 hover:bg-green-700 disabled:bg-gray-600 disabled:cursor-not-allowed text-white font-bold text-lg rounded-xl transition shadow-lg"
                        >
                            {loading ? "Procesando..." : "Finalizar Compra"}
                        </button>
                    </div>
                )}
            </main>
        </div>
    );
}
