import { createContext, useState, useContext } from "react";

export const CartContext = createContext();

export const CartProvider = ({ children }) => {
    const [cartItems, setCartItems] = useState([]);

    // Add item to cart
    const addToCart = (product, quantity = 1) => {
        setCartItems(prev => {
            const existing = prev.find(item => item.productId === product.id);

            if (existing) {
                return prev.map(item =>
                    item.productId === product.id
                        ? { ...item, quantity: item.quantity + quantity }
                        : item
                );
            }

            return [...prev, {
                productId: product.id,
                productName: product.name,
                unitPrice: product.unitPrice,
                quantity
            }];
        });
    };

    // Remove item from cart
    const removeFromCart = (productId) => {
        setCartItems(prev => prev.filter(item => item.productId !== productId));
    };

    // Update quantity
    const updateQuantity = (productId, quantity) => {
        if (quantity <= 0) {
            removeFromCart(productId);
            return;
        }

        setCartItems(prev =>
            prev.map(item =>
                item.productId === productId
                    ? { ...item, quantity }
                    : item
            )
        );
    };

    // Clear cart
    const clearCart = () => {
        setCartItems([]);
    };

    // Calculate totals
    const subtotal = cartItems.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0);
    const tax = subtotal * 0.19; // 19% tax
    const total = subtotal + tax;

    return (
        <CartContext.Provider value={{
            cartItems,
            addToCart,
            removeFromCart,
            updateQuantity,
            clearCart,
            subtotal,
            tax,
            total,
            itemCount: cartItems.reduce((sum, item) => sum + item.quantity, 0)
        }}>
            {children}
        </CartContext.Provider>
    );
};

// Custom hook
export const useCart = () => {
    const context = useContext(CartContext);
    if (!context) {
        throw new Error("useCart must be used within CartProvider");
    }
    return context;
};
