import { Component, EventEmitter, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { User } from 'src/app/_models/user';

export interface RoleInput {
  name: string;
  value: string;
  checked: boolean;
}

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {
  @Input() updateSelectedRoles: EventEmitter<RoleInput[]> = new EventEmitter<RoleInput[]>();
  user: User;
  roles: RoleInput[];

  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void { }

  updateRoles() {
    this.updateSelectedRoles.emit(this.roles);

    this.bsModalRef.hide();
  }
}
