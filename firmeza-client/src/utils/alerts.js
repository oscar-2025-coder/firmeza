import Swal from "sweetalert2";


export function showCenterAlert(message, type = "success") {
    Swal.fire({
        position: "center",
        icon: type,
        title: message,
        showConfirmButton: false,
        timer: 1800,
        background: "rgba(25, 25, 25, 0.9)",
        color: "#fff",
        backdrop: "rgba(0,0,0,0.3)",
    });
}


export function showError(message) {
    Swal.fire({
        position: "center",
        icon: "error",
        title: message,
        showConfirmButton: false,
        timer: 2000,
        background: "rgba(25, 25, 25, 0.9)",
        color: "#fff",
        backdrop: "rgba(0,0,0,0.3)",
    });
}


export function showSuccess(message) {
    Swal.fire({
        position: "center",
        icon: "success",
        title: message,
        showConfirmButton: false,
        timer: 2000,
        background: "rgba(25, 25, 25, 0.9)",
        color: "#fff",
        backdrop: "rgba(0,0,0,0.3)",
    });
}
