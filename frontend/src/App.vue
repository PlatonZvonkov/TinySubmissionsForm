<script setup>
import FormSubmit from './components/FormSubmit.vue';
import SubmissionsList from './components/SubmissionsList.vue';
</script>

<template>
  <div class="app">
    <h1>Tiny Forms — demo</h1>
    <div class="layout">
      <section class="left">
        <FormSubmit />
      </section>
      <section class="right">
        <SubmissionsList />
      </section>
    </div>
  </div>
</template>

<style scoped>
.app { 
  max-width: 1100px;
  margin: 20px auto;
  font-family: Arial, Helvetica, sans-serif;
}
.layout {
  display: flex;
  gap: 20px;
  }
.left { 
  flex: 1;
  }
.right {
  flex: 1;
  }
</style>