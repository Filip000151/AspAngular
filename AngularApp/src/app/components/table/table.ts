import { Component, inject, OnInit } from '@angular/core';
import { EmployeeWorkTime } from '../../Model/hoursWorked.type';
import { EmployeeService } from '../../services/employee';
import { Employee } from '../../Model/employee.type';
import { DecimalPipe } from '@angular/common';
import { PieChart } from '../pie-chart/pie-chart';

@Component({
  selector: 'app-table',
  imports: [DecimalPipe, PieChart],
  templateUrl: './table.html',
  styleUrl: './table.css'
})
export class Table implements OnInit{
  employeeService = inject(EmployeeService)
  employees: EmployeeWorkTime[] = [];

  ngOnInit(): void {
    this.employeeService.getEmployees().subscribe(entries => {
      this.filterData(entries);
    })
  }

  filterData(entries: Employee[]){
    const employeeMap = new Map<string, number>();

    entries.forEach(entry => {
      if(!entry.EmployeeName || entry.DeletedOn) return;

      const start = new Date(entry.StarTimeUtc);
      const end = new Date(entry.EndTimeUtc);
      const hours = (end.getTime() - start.getTime()) / (1000 * 60 * 60);

      if(hours > 0){
        employeeMap.set(entry.EmployeeName, (employeeMap.get(entry.EmployeeName) || 0) + hours);
      }
    })

    this.employees = Array.from(employeeMap.entries()).map(([EmployeeName, HoursWorked]) => ({EmployeeName, HoursWorked})).sort((a, b) => b.HoursWorked - a.HoursWorked);
  }
}
