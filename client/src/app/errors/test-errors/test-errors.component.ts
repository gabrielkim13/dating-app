import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {
  baseUrl = environment.apiUrl;
  validationErrors: string[] = [];

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  get400Error() {
    this.http.get(this.baseUrl + 'buggy/bad-request')
      .subscribe(response => console.log(response), error => console.log(error));
  }

  get400ValidationError() {
    this.http.post(this.baseUrl + 'account/register', { Username: '', Password: '123456789' })
      .subscribe(response => console.log(response), error => {
        console.log(error);
        this.validationErrors = error;
      });
  }

  get401Error() {
    this.http.get(this.baseUrl + 'buggy/auth')
      .subscribe(response => console.log(response), error => console.log(error));
  }

  get404Error() {
    this.http.get(this.baseUrl + 'buggy/not-found')
      .subscribe(response => console.log(response), error => console.log(error));
  }

  get500Error() {
    this.http.get(this.baseUrl + 'buggy/server-error')
      .subscribe(response => console.log(response), error => console.log(error));
  }
}
