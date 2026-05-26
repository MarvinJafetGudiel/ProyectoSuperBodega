import http from 'k6/http';

import { check, sleep } from 'k6';

export const options = {
    vus: 20,
    duration: '20s',
};

export default function () {

    const payload = JSON.stringify({
        clienteId: 1,

        productos: [
            {
                productoId: 1,
                cantidad: 1
            }
        ]
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    // PRUEBA SINCRÓNICA
    // RabbitMQ DESACTIVADO

    const respuesta = http.post(
        'http://localhost:5243/api/ventas',
        payload,
        params
    );

    check(respuesta, {
        'status es 200': (r) => r.status === 200,
    });

    sleep(1);
}