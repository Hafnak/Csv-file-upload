import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.css']
})
export class FileUploadComponent {
  selectedFile: File | null = null;
  successMessage: string = '';
  validCount: number = 0;
  invalidCount: number = 0;
  validRecords: any[] = [];

  constructor(private http: HttpClient) { }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0] as File;
  }

  onUpload() {
    if (!this.selectedFile) {
      console.error('No file selected.');
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);
    this.http.post<any>('https://localhost:7028/FileUpload/upload', formData).subscribe(
      response => {
        console.log(response);
        // Display success message
        if (response && response.validCount !== undefined && response.invalidCount !== undefined) {
          this.successMessage = `Data inserted successfully.`;
          this.validCount = response.validCount;
          this.invalidCount = response.invalidCount;
        }
      },
      error => {
        console.error(error);
        // Handle error 
        alert('An error occurred while uploading the file. Please try again ');
      }
    );
  }

  fetchValidRecords() {
    this.http.get<any[]>('https://localhost:7028/FileUpload/valid-records').subscribe(
      records => {
        this.validRecords = records;
      },
      error => {
        console.error(error);
        // Handle error 
        alert('An error occurred while fetching valid records.')
      }
    );
  }
}
