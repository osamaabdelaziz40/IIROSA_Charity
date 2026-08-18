import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[appHasPermission]'
})
export class HasPermissionDirective {
  @Input() set appHasPermission(permission: string) {
    // TODO: Implement permission check from user roles/claims
    // const hasPermission = this.checkPermission(permission);
    // if (hasPermission) {
    //   this.viewContainer.createEmbeddedView(this.templateRef);
    // } else {
    //   this.viewContainer.clear();
    // }
  }

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef
  ) {}
}
