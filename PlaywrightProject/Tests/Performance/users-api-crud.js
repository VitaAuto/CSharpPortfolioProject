import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = { vus: 10, duration: '30s' };

const BASE_URL = __ENV.API_BASE_URL || 'http://localhost:5043';
const LOGIN_ENDPOINT = '/api/Auth/login';
const USERS_ENDPOINT = '/api/users';

export function setup() {
    const loginPayload = JSON.stringify({
        username: __ENV.K6_USERNAME,
        password: __ENV.K6_PASSWORD,
    });
    const loginHeaders = { 'Content-Type': 'application/json' };
    const loginRes = http.post(`${BASE_URL}${LOGIN_ENDPOINT}`, loginPayload, { headers: loginHeaders });
    check(loginRes, { 'login status 200': (r) => r.status === 200 });
    const token = JSON.parse(loginRes.body).token;
    return { token };
}

export default function (data) {
    const authHeaders = {
        'Authorization': `Bearer ${data.token}`,
        'Content-Type': 'application/json',
    };

    // CREATE
    const userPayload = JSON.stringify({
        firstName: 'Test',
        lastName: 'User',
        email: `testuser${__VU}_${__ITER}@example.com`,
        isActive: true,
    });
    const createRes = http.post(`${BASE_URL}${USERS_ENDPOINT}`, userPayload, { headers: authHeaders });
    check(createRes, { 'create user status 201': (r) => r.status === 201 });

    if (createRes.status === 201) {
        const userId = JSON.parse(createRes.body).id;

        // READ
        const getRes = http.get(`${BASE_URL}${USERS_ENDPOINT}/${userId}`, { headers: authHeaders });
        check(getRes, { 'get user status 200': (r) => r.status === 200 });

        // UPDATE (PUT)
        const updatePayload = JSON.stringify({
            id: userId,
            firstName: 'Updated',
            lastName: 'User',
            email: `updateduser${__VU}_${__ITER}@example.com`,
            isActive: false,
        });
        const updateRes = http.put(`${BASE_URL}${USERS_ENDPOINT}/${userId}`, updatePayload, { headers: authHeaders });
        check(updateRes, { 'update user status 200': (r) => r.status === 200 });

        // PATCH
        const patchPayload = JSON.stringify({ email: `patcheduser${__VU}_${__ITER}@example.com` });
        const patchRes = http.patch(`${BASE_URL}${USERS_ENDPOINT}/${userId}`, patchPayload, { headers: authHeaders });
        check(patchRes, { 'patch user status 200': (r) => r.status === 200 });

        // DELETE
        const deleteRes = http.del(`${BASE_URL}${USERS_ENDPOINT}/${userId}`, null, { headers: authHeaders });
        check(deleteRes, { 'delete user status 200/204': (r) => r.status === 200 || r.status === 204 });
    }

    sleep(1);
}