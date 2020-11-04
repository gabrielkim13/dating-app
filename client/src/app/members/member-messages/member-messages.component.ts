import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import Message from 'src/app/_models/messages';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild("messageForm") messageForm: NgForm;

  @Input() messages: Message[];
  @Input() username: string;
  content: string = "";

  loading = false;

  constructor(public messageService: MessageService) { }

  ngOnInit(): void { }

  sendMessage() {
    this.loading = true;

    this.messageService.sendMessage(this.username, this.content)
      .then(() => {
        this.messageForm.reset();
      })
      .finally(() => this.loading = false);
  }
}
