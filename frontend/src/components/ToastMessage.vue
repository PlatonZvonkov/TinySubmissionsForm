<template>
  <div v-if="show" class="toast" :class="type">
    {{ message }}
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';

const props = defineProps({
  message: String,
  type: {
    type: String,
    default: 'error' // 'error', 'success', etc.
  },
  show: Boolean,
  duration: {
    type: Number,
    default: 5000
  }
});

const emit = defineEmits(['update:show']);
const show = ref(false);

watch(() => props.show, (newVal) => {
  show.value = newVal;
  if (newVal) {
    setTimeout(() => {
      show.value = false;
      emit('update:show', false);
    }, props.duration);
  }
});
</script>

<style scoped>
.toast {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 16px 24px;
  border-radius: 8px;
  color: white;
  font-weight: 500;
  z-index: 1000;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  animation: slideIn 0.3s ease-out;
}

.toast.error {
  background-color: #b00020;
}

.toast.success {
  background-color: #2e7d32;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}
</style>