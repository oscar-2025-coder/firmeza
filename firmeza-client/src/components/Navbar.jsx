import { useContext } from "react";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";
import { useCart } from "../context/CartContext";

export default function Navbar() {
    const { user, logout } = useContext(AuthContext);
    const { itemCount } = useCart();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate("/");
    };

    return (
        <header className="bg-[#1A1A1A] border-b border-gray-800 py-4 px-6 sticky top-0 z-50">
            <div className="max-w-7xl mx-auto flex justify-between items-center">
                {/* Logo/Brand */}
                <div
                    className="text-2xl font-bold cursor-pointer hover:text-blue-400 transition"
                    onClick={() => navigate("/products")}
                >
                    Firmeza S.A.S
                </div>

                {/* User Menu */}
                <div className="flex items-center gap-4">
                    {user && (
                        <div className="hidden md:flex items-center gap-2 text-gray-300">
                            <span className="text-sm">Hola,</span>
                            <span className="font-semibold text-blue-400">
                                {user.email}
                            </span>
                        </div>
                    )}

                    {/* Cart Button */}
                    <button
                        onClick={() => navigate("/cart")}
                        className="relative px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded-lg transition font-semibold"
                    >
                        <span className="hidden sm:inline">🛒 Carrito</span>
                        <span className="sm:hidden">🛒</span>
                        {itemCount > 0 && (
                            <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full w-6 h-6 flex items-center justify-center font-bold">
                                {itemCount}
                            </span>
                        )}
                    </button>

                    {/* Logout Button */}
                    <button
                        onClick={handleLogout}
                        className="px-4 py-2 bg-red-600 hover:bg-red-700 rounded-lg transition font-semibold"
                    >
                        <span className="hidden sm:inline">Cerrar Sesión</span>
                        <span className="sm:hidden">Salir</span>
                    </button>
                </div>
            </div>
        </header>
    );
}
