import {AfterViewInit, Component, ElementRef, ViewChild} from '@angular/core';
import {BehaviorSubject} from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements AfterViewInit{
  angular = false;
  razor = false;
  clientHeight = '80vh';

  @ViewChild('host')
  host: ElementRef | null = null;

  constructor() { }

  updateHeight(height: any): void {
    if (height !== 0 && height !== undefined) {
      if (this.clientHeight !== height + 'px') {
        this.clientHeight = height + 'px';
      }
    }
  }

  ngAfterViewInit(): void {
    if (this.host) {
      this.updateHeight(this.host.nativeElement.clientHeight);
    }
  }
}
