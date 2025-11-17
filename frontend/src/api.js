import axios from "axios";

const API_BASE = import.meta.env.VITE_API_BASE ||  'http://localhost:5005/api';

export async function submitForm(payload, timeoutMs = 5000) {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeoutMs);
    
    try {
        const result = await axios.post(`${API_BASE}/submissions`, payload, {
            signal: controller.signal
        });
        return result;
    } catch (err) {
        if (axios.isCancel(err)) {
            throw new Error('Request timed out');
        }
        throw err;
    } finally {
        clearTimeout(timeoutId);
    }
}

export async function fetchSubmissions({formType, search, page = 1, pageSize = 50}={}){
    const params = {};
    const controller = createTimeoutController(10000);
    if (formType) {
        params.formType = formType;
    }
    if(search){
        params.search = search
    }
    params.page = page;
    params.pageSize = pageSize;
     try {
        const result = await axios.get(`${API_BASE}/submissions`, {
            params,
            signal: controller.signal
        });
        return result.data;
    } catch (err) {
        if (axios.isCancel(err)) {
            console.warn("Request cancelled: fetchSubmissions()", err.message);
        }
        throw err;
    } finally {
        controller.cleanup();
    }
}

function createTimeoutController(ms = 10000) {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), ms);

    controller.cleanup = () => clearTimeout(timeoutId);

    return controller;
}