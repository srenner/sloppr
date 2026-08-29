import { defineConfig } from 'orval';

export default defineConfig({
  api: {
    input: 'http://localhost:5220/openapi/v1.json',
    output: {
      mode: 'tags-split',
      target: 'src/app/api/generated.ts',
      schemas: 'src/app/api/model',
      client: 'angular',
      override: {
        angular: {
          provideIn: 'root',
        },
        mutator: {
          path: './src/app/api/custom-instance.ts',
          name: 'customInstance',
        },
      },
    },
  },
});