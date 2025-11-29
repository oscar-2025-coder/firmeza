import { useEffect } from "react";
import Swal from "sweetalert2";

export default function LandingPage() {

    // ⭐ ALERTA MODERNA Y CENTRADA (visible, elegante, NO tapa la UI)
    useEffect(() => {
        Swal.fire({
            position: "center",
            icon: "info",
            title: "Bienvenido a Firmeza S.A.S 👷‍♂️",
            text: "Materiales de construcción y maquinaria pesada en un solo lugar.",
            showConfirmButton: false,
            timer: 2200,
            background: "rgba(25,25,25,0.92)",
            color: "#fff",
            backdrop: "rgba(0,0,0,0.35)", // suave y elegante
        });
    }, []);

    return (
        <div
            className="h-screen w-full bg-cover bg-center relative animate-fadeIn"
            style={{
                backgroundImage:
                    "url('https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1920&q=80')",
            }}
        >

            {/* Overlay suave */}
            <div className="absolute inset-0 bg-black/60 backdrop-blur-sm"></div>

            {/* Contenido principal */}
            <div className="absolute inset-0 flex items-center justify-center px-6">

                <div className="
                    text-center bg-white/10 backdrop-blur-lg 
                    p-10 rounded-3xl shadow-xl max-w-2xl
                    transform transition-all animate-slideUp
                    border border-white/20
                ">

                    {/* Título premium con gradiente */}
                    <h1 className="
                        text-5xl font-extrabold mb-4 
                        bg-gradient-to-r from-white via-gray-200 to-gray-400 
                        bg-clip-text text-transparent
                        drop-shadow-[0_2px_10px_rgba(255,255,255,0.4)]
                    ">
                        Firmeza S.A.S
                    </h1>

                    {/* Subtítulo moderno */}
                    <p className="text-gray-200 text-lg md:text-xl mb-8 leading-relaxed">
                        Materiales de construcción y maquinaria pesada para tus proyectos.
                        <br />
                        Servicio rápido, confiable y al mejor precio.
                    </p>

                    {/* Botones modernos */}
                    <div className="flex justify-center gap-6 mt-4">

                        <a href="/login">
                            <button className="
                                px-8 py-3 rounded-xl text-white font-semibold 
                                bg-blue-600/80 hover:bg-blue-700 
                                backdrop-blur-lg shadow-lg hover:shadow-xl
                                transition-transform hover:-translate-y-1
                                border border-blue-300/20
                            ">
                                Iniciar Sesión
                            </button>
                        </a>

                        <a href="/register">
                            <button className="
                                px-8 py-3 rounded-xl text-white font-semibold 
                                bg-green-600/80 hover:bg-green-700 
                                backdrop-blur-lg shadow-lg hover:shadow-xl
                                transition-transform hover:-translate-y-1
                                border border-green-300/20
                            ">
                                Registrarse
                            </button>
                        </a>

                    </div>

                </div>

            </div>
        </div>
    );
}
