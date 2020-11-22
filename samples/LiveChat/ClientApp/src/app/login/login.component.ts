import { Component } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { ConnectionService } from '../services/connectionService';

@Component({
  selector: 'app-home',
  templateUrl: './login.component.html',
  styleUrls: [ './login.component.css' ]
})
export class LoginComponent {
  form: FormGroup = new FormGroup({
    name: new FormControl('')
  });

  constructor(private connectionService: ConnectionService) {

  }

  async connect() {
    if (this.form.valid) {
      await this.connectionService.connect(this.form.get("name").value);
    }
  }
}
