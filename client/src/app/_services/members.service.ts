import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../environments/environment';
import Member from '../_models/member';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  members: Member[] = [];

  constructor(private http: HttpClient) { }

  getMembers() {
    if (this.members.length > 0) return of(this.members);

    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(map(members => this.members = members));
  }

  getMember(username: string) {
    const member = this.members.find(member => member.userName === username);

    if (!!member) return of(member);

    return this.http.get<Member>(this.baseUrl + `users/${username}`).pipe(map(member => {
      this.members.push(member);

      return member;
    }));
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(map(() => {
      this.members = this.members.map(mapMember => mapMember.id === member.id ? member : mapMember);
    }));
  }
}
