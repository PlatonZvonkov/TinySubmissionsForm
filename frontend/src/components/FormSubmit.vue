<template>
  <div class="form-card">
    <h2>Order Form</h2>
    <form @submit.prevent="onSubmit">
      <div class="input">
        <label for="name">Name *</label>
        <input id="name" v-model="form.name" type="text" />
        <div v-if="errors.name" class="small" style="color:#b00020">{{ errors.name }}</div>
      </div>

      <div class="input">
        <label for="email">Email *</label>
        <input id="email" v-model="form.email" type="text" />
        <div v-if="errors.email" class="small" style="color:#b00020">{{ errors.email }}</div>
      </div>

      <div class="input">
        <label for="product">Product</label>
        <select id="product" v-model="form.product">
          <option value="">Select a product</option>
          <option value="alpha">Alpha</option>
          <option value="beta">Beta</option>
          <option value="gamma">Gamma</option>          
        </select>
        <div v-if="errors.product" class="small" style="color:#b00020">{{ errors.product }}</div>
      </div>

      <div class="input">
        <label>Delivery Type</label>
        <label style="margin-left:12px"><input type="radio" value="standard" v-model="form.delivery" /> Standard</label>
        <label style="margin-left:12px"><input type="radio" value="express" v-model="form.delivery" /> Express</label>
      </div>

      <div class="input">
        <label for="date">Preferred Date</label>
        <input id="date" type="date" v-model="form.date" />
        <div v-if="errors.date" class="small" style="color:#b00020">{{ errors.date }}</div>
      </div>

      <div class="input">
        <label><input type="checkbox" v-model="form.confirm" /> I confirm the information is correct</label>
        <div v-if="errors.confirm" class="small" style="color:#b00020">{{ errors.confirm }}</div>
      </div> 
   
      <div style="display:flex; flex-direction: column-reverse; gap:8px; align-items:end;">
        <button type="submit" :disabled="isSubmitting">
          {{ isSubmitting ? 'Loading...' : 'Submit' }}
        </button>
        
      </div>
    </form>    
  </div>
  <ToastMessage 
      v-model:show="showToast" 
      :message="toastMessage" 
      type="error" 
    />
</template>

<script setup>
import { reactive, ref } from 'vue';
import { submitForm } from '../api';
import ToastMessage from './ToastMessage.vue';

const form = reactive({
  name: '',
  email: '',
  product: '',
  delivery: 'standard',
  confirm: false,
  date: ''
});

const errors = reactive({
  name: '',
  email: '',
  product: '',
  confirm: '',
  date: ''
});

const status = ref('');
const isSubmitting = ref(false);
const showToast = ref(false);
const toastMessage = ref('');

const submissionTimeout = ref(null);

async function onSubmit() {
  status.value = '';
  isSubmitting.value = true;
  showToast.value = false;
  
  if (!validate()) {
    isSubmitting.value = false;
    return;
  }

  const payload = {
    formType: 'contact-order',
    payload: {
      name: form.name,
      email: form.email,
      product: form.product,
      delivery: form.delivery,
      date: form.date,
      confirm: form.confirm
    }
  };

  try {
    status.value = 'Submitting...';
        
    submissionTimeout.value = setTimeout(() => {
      if (isSubmitting.value) {
        toastMessage.value = 'Something went wrong - request is taking longer than expected';
        showToast.value = true;
      }
    }, 5000); 

    await submitForm(payload);
    
    clearTimeout(submissionTimeout.value);
    
    status.value = 'Submitted!';

    // Reset form
    form.name = '';
    form.email = '';
    form.product = '';
    form.delivery = 'standard';
    form.confirm = false;
    form.date = '';
    
    window.dispatchEvent(new Event('submissions:changed'));
  } catch (err) {
    clearTimeout(submissionTimeout.value);
    
    console.error(err);
    
    if (err.message === 'Request timed out') {
      status.value = 'Request timed out';
      toastMessage.value = 'Something went wrong - request timed out';
    } else {
      status.value = 'Error submitting';
      toastMessage.value = 'Something went wrong - please try again';
    }
    
    showToast.value = true;
  } finally {
    isSubmitting.value = false;
    setTimeout(() => {
      status.value = '';
      showToast.value = false;
    }, 2500);
  }
}

function validate() {
  let ok = true;
  errors.name = '';
  errors.email = '';
  errors.product = '';
  errors.confirm = '';
  errors.date = '';

  if (!form.name || form.name.trim().length < 2) {
    errors.name = 'Name must be at least 2 characters';
    ok = false;
  } else if (/^\d+$/.test(form.name.trim())) {
    errors.name = 'Name cannot consist only of numbers.';
    ok = false;
  }

  if (!form.email || !/^\S+@\S+\.\S+$/.test(form.email)) {
    errors.email = 'Valid email required';
    ok = false;
  }

  if(!form.product){
    errors.product = 'Please select a product.';
    ok = false;
  }

  if (!form.confirm) {
    errors.confirm = 'You must confirm the information';
    ok = false;
  }

  if (!form.date){
    errors.date = 'Preferred date is required.';
    ok = false;
  }

  return ok;
}
</script>
