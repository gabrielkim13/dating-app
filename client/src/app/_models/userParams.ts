import { User } from './user';

export class UserParams {
  gender: string;
  minAge: number;
  maxAge: number;
  pageNumber = 1;
  pageSize = 5;
  orderBy = 'lastActive';

  constructor(user: User) {
    this.gender = user.gender == 'female' ? 'male' : 'female';

    // Half age plus seven rule:
    this.minAge = Math.round(user.age / 2 + 7);
    this.minAge = this.minAge < 18 ? 18 : this.minAge;
    this.maxAge = 2 * (user.age - 7);
  }
}
