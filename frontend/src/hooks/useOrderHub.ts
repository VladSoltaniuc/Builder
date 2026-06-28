// Service layer — SignalR connection for live order updates
import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { getToken } from "../auth/token";
import type { Order } from "../types/order";

export function useOrderHub(onOrderChanged: (order: Order) => void) {
  // Keep the callback ref fresh every render so the SignalR handler never
  // closes over a stale version of the caller's state.
  const callbackRef = useRef(onOrderChanged);
  callbackRef.current = onOrderChanged;

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_STATIC_BASE_URL}/hubs/orders`, {
        // SignalR WebSocket handshakes can't set Authorization headers;
        // the client sends the JWT as ?access_token= in the query string instead.
        accessTokenFactory: () => getToken() ?? "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on("OrderStatusChanged", (order: Order) => {
      callbackRef.current(order);
    });

    connection.start().catch(() => {
      // Non-fatal — the table still works; live push just won't arrive.
    });

    return () => { void connection.stop(); };
  }, []); // connection is stable for the lifetime of this component mount
}
