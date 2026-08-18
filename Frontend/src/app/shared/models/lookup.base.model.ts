export class LookupBase {
  id: any = null;
  name: any = null;
  systemValue?: any = null;
  disabled?: boolean = false;
  constructor() {
    this.id = null;
    this.name = null;
    this.systemValue = null;
    this.disabled = false;
  }
}

export class LookupModel extends LookupBase {
  nameAr: string = '';
  nameEn: string = '';

  constructor() {
    super()
  }
}
