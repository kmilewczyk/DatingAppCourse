import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[];

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.loadPhotos();
  }

  loadPhotos() {
    this.adminService.getUnapprovedPhotos().subscribe(photos => {
      this.photos = photos;
    })
  }

  rejectPhoto(id: number) {
    this.adminService.rejectPhoto(id).subscribe(() => {
      this.photos = this.photos.filter(x => x.id != id);
    })
  }

  approvePhoto(id: number) {
    this.adminService.approvePhoto(id).subscribe(() => {
      this.photos = this.photos.filter(x => x.id != id);
    })
  }
}
