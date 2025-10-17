import { Component, computed, input } from '@angular/core';
import { EmployeeWorkTime } from '../../Model/hoursWorked.type';
import { IgxPieChartModule } from 'igniteui-angular-charts';


@Component({
  selector: 'app-pie-chart',
  imports: [IgxPieChartModule],
  templateUrl: './pie-chart.html',
  styleUrl: './pie-chart.css'
})
export class PieChart {
  employees = input.required<EmployeeWorkTime[]>();
  
  chartData = computed(() => {
    const total = this.employees().reduce((sum, emp) => sum + emp.HoursWorked, 0);
    return this.employees().map(emp => ({
      name: emp.EmployeeName,
      totalTimeWorked: emp.HoursWorked,
      percentage: Math.round((emp.HoursWorked / total) * 100) + '%'
    }));
  });

  colors = [
    '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', 
    '#9966FF', '#FF9F40', '#8AC926', '#1982C4'
  ];
  
}
