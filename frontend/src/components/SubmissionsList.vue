<template>
  <div>
    <div class="form-card">
      <h2>Submissions</h2>
      <div class="search-row">
        <input class="search" v-model="search" placeholder="search..." />
        <!-- <input v-model="filterForm" placeholder="formType filter" /> -->
        <button @click="load">Search</button>
        <button @click="clear">Clear</button>
      </div>

      <div v-if="loading" class="small">Loading...</div>

      <div v-for="s in submissions" :key="s.id" class="submission">
        <div><strong>{{ s.FormType }}</strong> <span class="small">— {{ formatDate(s.SubmittedAt) }}</span></div>
        <div class="small" v-html="payloadPretty(s.Payload)"></div>
      </div>

      <div style="margin-top:12px;">
        <button @click="prev" :disabled="page === 1">Prev</button>
        <span class="small">Page {{ page }}</span>
        <button @click="next" :disabled="submissions.length < pageSize">Next</button>
      </div>
    </div>
  </div>
</template>


<script setup>
import { ref, onMounted } from 'vue';
import { fetchSubmissions } from '../api';

const submissions = ref([]);
const search = ref('');
const filterForm = ref('');
const page = ref(1);
const pageSize = ref(20);
const loading = ref(false);

async function load() {
  loading.value = true;
  try {
    const data = await fetchSubmissions({ formType: filterForm.value || undefined, search: search.value || undefined, page: page.value, pageSize: pageSize.value });
    submissions.value = data;
  } catch (err) {
    console.error(err);
  } finally {
    loading.value = false;
  }
}

function formatDate(dt) {
  try {
    return new Date(dt).toLocaleString();
  } catch {
    return dt;
  }
}

function payloadPretty(blobStr) {
  if (!blobStr) return "—";

  let obj;
  try {
    obj = typeof blobStr === "string" ? JSON.parse(blobStr) : blobStr;
  } catch {
    return blobStr; // fallback
  }

  const friendly = [];

  if (obj.name) friendly.push(`<strong>${obj.name}</strong>`);
  if (obj.email) friendly.push(`<div>${obj.email}</div>`);

  if (obj.product) friendly.push(`<div>Product: <strong>${obj.product}</strong></div>`);
  if (obj.delivery) friendly.push(`<div>Delivery: <strong>${obj.delivery}</strong></div>`);

  if (obj.date) {
    const d = new Date(obj.date);
    const formatted = isNaN(d) ? obj.date : d.toLocaleDateString();
    friendly.push(`<div>Date: ${formatted}</div>`);
  }

  if (obj.confirm !== undefined)
    friendly.push(`<div>Confirmed: ${obj.confirm ? "Yes" : "No"}</div>`);

  return friendly.join("");
}


function prev() {
  if (page.value > 1) {
    page.value--;
    load();
  }
}
function next() {
  page.value++;
  load();
}

function clear() {
  search.value = '';
  filterForm.value = '';
  page.value = 1;
  load();
}

onMounted(() => {
  load();
  
  // listen for new submissions to refresh
  window.addEventListener('submissions:changed', load);
});
</script>

<style scoped>
pre { 
    font-size: 0.9rem;
    margin: 0; }
</style>