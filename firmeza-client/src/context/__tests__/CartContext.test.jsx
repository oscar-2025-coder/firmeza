import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { CartProvider, useCart } from '../CartContext';

describe('CartContext', () => {
    it('should calculate subtotal, tax, and total correctly', () => {
        const { result } = renderHook(() => useCart(), {
            wrapper: CartProvider,
        });

        // Add test products to cart
        act(() => {
            result.current.addToCart(
                { id: '1', name: 'Product 1', unitPrice: 10000, stock: 10, sku: 'P1', isActive: true },
                2
            );
            result.current.addToCart(
                { id: '2', name: 'Product 2', unitPrice: 5000, stock: 10, sku: 'P2', isActive: true },
                3
            );
        });

        // Verify calculations
        // Cart: 2x 10000 + 3x 5000 = 20000 + 15000 = 35000 (subtotal)
        // Tax: 35000 * 0.19 = 6650
        // Total: 35000 + 6650 = 41650

        expect(result.current.subtotal).toBe(35000);
        expect(result.current.tax).toBe(6650);
        expect(result.current.total).toBe(41650);
        expect(result.current.itemCount).toBe(5); // 2 + 3
    });

    it('should update quantity correctly', () => {
        const { result } = renderHook(() => useCart(), {
            wrapper: CartProvider,
        });

        act(() => {
            result.current.addToCart(
                { id: '1', name: 'Product 1', unitPrice: 10000, stock: 10, sku: 'P1', isActive: true },
                2
            );
        });

        expect(result.current.itemCount).toBe(2);

        // Update quantity to 5
        act(() => {
            result.current.updateQuantity('1', 5);
        });

        expect(result.current.itemCount).toBe(5);
        expect(result.current.subtotal).toBe(50000);
    });

    it('should remove item correctly', () => {
        const { result } = renderHook(() => useCart(), {
            wrapper: CartProvider,
        });

        act(() => {
            result.current.addToCart(
                { id: '1', name: 'Product 1', unitPrice: 10000, stock: 10, sku: 'P1', isActive: true },
                2
            );
            result.current.addToCart(
                { id: '2', name: 'Product 2', unitPrice: 5000, stock: 10, sku: 'P2', isActive: true },
                3
            );
        });

        expect(result.current.cartItems.length).toBe(2);

        act(() => {
            result.current.removeFromCart('1');
        });

        expect(result.current.cartItems.length).toBe(1);
        expect(result.current.subtotal).toBe(15000);
    });

    it('should clear cart correctly', () => {
        const { result } = renderHook(() => useCart(), {
            wrapper: CartProvider,
        });

        act(() => {
            result.current.addToCart(
                { id: '1', name: 'Product 1', unitPrice: 10000, stock: 10, sku: 'P1', isActive: true },
                2
            );
        });

        expect(result.current.cartItems.length).toBe(1);

        act(() => {
            result.current.clearCart();
        });

        expect(result.current.cartItems.length).toBe(0);
        expect(result.current.subtotal).toBe(0);
        expect(result.current.total).toBe(0);
    });
});
