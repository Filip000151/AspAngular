import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Employee } from '../Model/employee.type';

@Injectable({
  providedIn: 'root'
})

export class EmployeeService{
  http = inject(HttpClient)
  getEmployees(){
    const url = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ=="
    return this.http.get<Array<Employee>>(url)
  }

}
