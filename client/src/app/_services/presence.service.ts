import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { filter, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  baseUrl = environment.hubUrl;

  private hubConnection: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.baseUrl + "presence", {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.error(error));

    this.hubConnection.on("UserIsOnline", (username: string) => {
      this.onlineUsers$.pipe(take(1)).subscribe((usernames: string[]) => {
        this.onlineUsersSource.next([...usernames, username]);
      })
    })

    this.hubConnection.on("UserIsOffline", (username: string) => {
      this.onlineUsers$.pipe(take(1)).subscribe((usernames: string[]) => {
        this.onlineUsersSource.next([...usernames.filter(filterUsername => filterUsername !== username)]);
      });
    })

    this.hubConnection.on("GetOnlineUsers", (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    })

    this.hubConnection.on("NewMessageReceived", ({ username, knownAs }: { username: string, knownAs: string }) => {
      this.toastr.info(`${knownAs} has sent you a new message!`)
        .onTap
        .pipe(take(1))
        .subscribe(() => {
          this.router.navigateByUrl(`/members/${username}?tab=3`);
        });
    })
  }

  stopHubConnection() {
    this.hubConnection.stop();
  }
}
