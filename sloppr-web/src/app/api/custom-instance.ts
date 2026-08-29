import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../environments/environment';

export const customInstance = <T>(config: {
  url: string;
  method: string;
  params?: any;
  data?: any;
}): any => {
  const http = inject(HttpClient);
  return http.request<T>(config.method, `${environment.apiUrl}${config.url}`, {
    params: config.params,
    body: config.data,
  });
};