import { useState } from "react";
import { registerUser } from "../api/auth";
import { useNavigate } from "react-router-dom";

// Alertas modernas
import { showError, showSuccess } from "../utils/alerts";

export default function RegisterPage() {
    const navigate = useNavigate();

    const [form, setForm] = useState({
        fullName: "",
        documentNumber: "",
        email: "",
        phoneNumber: "",
        age: "",
        password: ""
    });

    const [loading, setLoading] = useState(false);

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Validación simple
        if (
            !form.fullName.trim() ||
            !form.documentNumber.trim() ||
            !form.email.trim() ||
            !form.phoneNumber.trim() ||
            !form.age ||
            !form.password.trim()
        ) {
            showError("Todos los campos son obligatorios");
            return;
        }

        setLoading(true);

        try {
            await registerUser({
                fullName: form.fullName,
                documentNumber: form.documentNumber,
                email: form.email,
                phoneNumber: form.phoneNumber,
                age: Number(form.age),
                password: form.password
            });

            showSuccess("Cuenta creada exitosamente ✔");

            setTimeout(() => navigate("/login"), 1500);

        } catch (err) {
            showError(err.response?.data || "Error al registrarse");
        }

        setLoading(false);
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-[#070B1A] to-[#111827] px-4">

            <form
                onSubmit={handleSubmit}
                className="
                    w-full max-w-lg 
                    bg-white/10 
                    backdrop-blur-2xl 
                    border border-white/20 
                    shadow-2xl 
                    rounded-2xl 
                    p-10
                    animate-fadeIn
                "
            >
                <h2 className="text-3xl font-bold text-center mb-6 text-white tracking-wide">
                    Crear Cuenta
                </h2>

                <div className="grid grid-cols-1 gap-5">

                    {/* Nombre */}
                    <input
                        type="text"
                        name="fullName"
                        placeholder="Nombre completo"
                        value={form.fullName}
                        onChange={handleChange}
                        className="
                            w-full px-4 py-3 
                            bg-[#0d0d0d]/70 text-white 
                            border border-gray-700 
                            rounded-xl 
                            focus:ring-2 focus:ring-blue-500
                            outline-none
                        "
                        required
                    />

                    {/* Documento */}
                    <input
                        type="text"
                        name="documentNumber"
                        placeholder="Documento"
                        value={form.documentNumber}
                        onChange={handleChange}
                        className="
                            w-full px-4 py-3 
                            bg-[#0d0d0d]/70 text-white 
                            border border-gray-700 
                            rounded-xl 
                            focus:ring-2 focus:ring-blue-500
                            outline-none
                        "
                        required
                    />

                    {/* Correo */}
                    <input
                        type="email"
                        name="email"
                        placeholder="Correo"
                        value={form.email}
                        onChange={handleChange}
                        className="
                            w-full px-4 py-3 
                            bg-[#0d0d0d]/70 text-white 
                            border border-gray-700 
                            rounded-xl 
                            focus:ring-2 focus:ring-blue-500
                            outline-none
                        "
                        required
                    />

                    {/* Teléfono */}
                    <input
                        type="text"
                        name="phoneNumber"
                        placeholder="Teléfono"
                        value={form.phoneNumber}
                        onChange={handleChange}
                        className="
                            w-full px-4 py-3 
                            bg-[#0d0d0d]/70 text-white 
                            border border-gray-700 
                            rounded-xl 
                            focus:ring-2 focus:ring-blue-500
                            outline-none
                        "
                        required
                    />

                    {/* Edad */}
                    <input
                        type="number"
                        name="age"
                        placeholder="Edad"
                        value={form.age}
                        onChange={handleChange}
                        className="
                            w-full px-4 py-3 
                            bg-[#0d0d0d]/70 text-white 
                            border border-gray-700 
                            rounded-xl 
                            focus:ring-2 focus:ring-blue-500
                            outline-none
                        "
                        required
                    />

                    {/* Contraseña */}
                    <input
                        type="password"
                        name="password"
                        placeholder="Contraseña"
                        value={form.password}
                        onChange={handleChange}
                        className="
                            w-full px-4 py-3 
                            bg-[#0d0d0d]/70 text-white 
                            border border-gray-700 
                            rounded-xl 
                            focus:ring-2 focus:ring-blue-500
                            outline-none
                        "
                        required
                    />

                </div>

                <button
                    type="submit"
                    disabled={loading}
                    className="
                        w-full mt-6 py-3 
                        bg-gradient-to-r from-blue-600 to-blue-700 
                        text-white font-semibold 
                        rounded-xl 
                        shadow-lg shadow-blue-900/40
                        hover:opacity-90 
                        transition 
                        text-lg
                        disabled:opacity-50
                    "
                >
                    {loading ? "Procesando..." : "Registrarse"}
                </button>

                <p className="text-center text-gray-400 mt-6 text-sm">
                    ¿Ya tienes cuenta?{" "}
                    <a href="/login" className="text-blue-400 hover:underline font-semibold">
                        Inicia sesión
                    </a>
                </p>
            </form>

        </div>
    );
}
