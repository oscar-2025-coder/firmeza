import { useState, useContext } from "react";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

// ALERTAS MODERNAS Y CENTRADAS
import { showError, showSuccess } from "../utils/alerts";

export default function LoginPage() {

    const navigate = useNavigate();
    const { login } = useContext(AuthContext);

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);

    const handleLogin = async () => {
        if (!email.trim() || !password.trim()) {
            showError("Debes completar todos los campos");
            return;
        }

        setLoading(true);

        try {
            await login(email, password);
            showSuccess("Bienvenido 👋");

            setTimeout(() => {
                navigate("/products");
            }, 1200);

        } catch (err) {
            showError("Credenciales incorrectas o error en el servidor");
        }

        setLoading(false);
    };

    return (
        <div
            className="
                flex items-center justify-center 
                min-h-screen 
                bg-gradient-to-br from-[#070B1A] to-[#111827]
                px-4
            "
        >
            <div
                className="
                    w-full max-w-md 
                    bg-white/10 
                    backdrop-blur-2xl 
                    border border-white/20 
                    shadow-2xl 
                    rounded-2xl 
                    p-10 
                    animate-fadeIn
                "
            >
                {/* Título */}
                <h2 className="text-center text-3xl font-extrabold text-white mb-8 tracking-wide">
                    Iniciar Sesión
                </h2>

                {/* Campo Email */}
                <div className="mb-6">
                    <label className="text-gray-300 text-sm font-medium">Email</label>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="
                            w-full px-4 py-3 mt-1 
                            bg-[#0d0d0d]/70 
                            text-white 
                            border border-gray-700 
                            rounded-xl 
                            focus:ring-2 focus:ring-blue-500 
                            outline-none
                            transition
                        "
                        placeholder="correo@ejemplo.com"
                    />
                </div>

                {/* Campo Password */}
                <div className="mb-8">
                    <label className="text-gray-300 text-sm font-medium">Password</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="
                            w-full px-4 py-3 mt-1 
                            bg-[#0d0d0d]/70 
                            text-white 
                            border border-gray-700 
                            rounded-xl 
                            focus:ring-2 focus:ring-blue-500 
                            outline-none
                            transition
                        "
                        placeholder="••••••••"
                    />
                </div>

                {/* Botón */}
                <button
                    onClick={handleLogin}
                    disabled={loading}
                    className="
                        w-full py-3 
                        rounded-xl 
                        text-white font-semibold text-lg 
                        bg-gradient-to-r from-blue-600 to-blue-700 
                        shadow-lg shadow-blue-900/40
                        hover:opacity-90 
                        transition 
                        disabled:opacity-50
                    "
                >
                    {loading ? (
                        <span className="animate-pulse">Ingresando...</span>
                    ) : (
                        "Entrar"
                    )}
                </button>

                {/* Enlace Registro */}
                <p className="mt-6 text-center text-gray-400 text-sm">
                    ¿No tienes cuenta?{" "}
                    <a
                        href="/register"
                        className="text-blue-400 font-semibold hover:underline"
                    >
                        Regístrate
                    </a>
                </p>
            </div>
        </div>
    );
}
