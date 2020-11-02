import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RoleInput, RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]> = [];
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe(users => {
      this.users = users;
    });
  }

  openRolesModal(user: User) {
    const config = {
      class: "modal-dialog-centered",
      initialState: {
        user,
        roles: this.getRolesArray(user)
      }
    } as ModalOptions;

    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.content.updateSelectedRoles.subscribe((roles: RoleInput[]) => {
      const rolesToUpdate = roles.filter(role => role.checked).map(role => role.name);

      if (rolesToUpdate) {
        this.adminService.updateUserRoles(user.username, rolesToUpdate).subscribe(() => {
          user.roles = rolesToUpdate;
        });
      }
    });
  }

  private getRolesArray(user: User) {
    const availableRoles = ['Admin', 'Moderator', 'Member'];

    return availableRoles.map(role => ({
      name: role,
      value: role,
      checked: user.roles.includes(role)
    }));
  }
}
