import { Component, Input, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { MemberDetailComponent } from '../member-detail/member-detail.component';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
})
export class MemberCardComponent implements OnInit {
  @Input() member: Member;

  constructor(private memberService: MembersService, private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  addLike(member: Member) {
    this.memberService.addLike(member.username).subscribe(() => {
      if (member.liked) {
        this.toastr.success('You have unliked  ' + member.knownAs);
        member.liked = false;
      } else {
        this.toastr.success('You have liked  ' + member.knownAs);
        member.liked = true;
      }
    });
  }

}
