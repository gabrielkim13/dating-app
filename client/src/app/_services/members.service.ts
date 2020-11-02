import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';

import { environment } from '../../environments/environment';
import Member from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationParams } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  user: User;
  userParams: UserParams;

  members: Member[] = [];
  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();

  memberCache = new Map<string, PaginatedResult<Member[]>>();

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);

    return this.userParams;
  }

  getMembers(userParams: UserParams) {
    const key = Object.values(userParams).join('-');

    const response = this.memberCache.get(key);

    if (response) {
      return of(response);
    }

    let params = getPaginationParams(userParams.pageNumber, userParams.pageSize);

    params = params.append('gender', userParams.gender);
    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());

    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(map(response => {
      this.memberCache.set(key, response);
      return response;
    }));
  }

  getMember(username: string) {
    const member = [...this.memberCache.values()]
      .reduce((members, paginatedResult) => {
        return members.concat(paginatedResult.result);
      }, [] as Member[])
      .find(findMember => findMember.userName === username);

    if (member) {
      return of(member);
    }

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

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + `users/set-main-photo/${photoId}`, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + `users/delete-photo/${photoId}`);
  }

  addLike(username: string) {
    return this.http.post(this.baseUrl + `likes/${username}`, {});
  }

  getLikes(predicate: ("liked" | "likedBy"), pageNumber: number, pageSize: number) {
    let params = getPaginationParams(pageNumber, pageSize);

    params = params.append('predicate', predicate);

    return getPaginatedResult<Partial<Member[]>>(this.baseUrl + `likes?predicate=${predicate}`, params, this.http);
  }
}
