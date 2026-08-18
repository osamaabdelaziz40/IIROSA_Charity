export { LookupBase, LookupModel } from './lookup.base.model';

export interface AttachmentDto {
  id?: string;
  fileName?: string;
  extension?: string;
  filePath?: string;
  contentType?: string;
  size?: number;
  fileContent?: any;
  fileData?: any;
  thumbnail?: string;
  isNew?: boolean;
  isDeleted?: boolean;
  description?: string;
  createdOn?: string | null;
}

export enum AttachmentFileType {
  General = 1,
  Document = 2,
  Image = 3,
  Audio = 4,
  Video = 5,
  PDF = 6,
  Word = 8,
  PowerPoint = 9,
  Excel = 12
}

export function customAttachmentValidator(): ValidatorFn {
  return (control: AbstractControl): { [key: string]: any } | null => {
    const attachmentList = control.value as AttachmentDto[];
    if (!Array.isArray(attachmentList) || !attachmentList.length) {
      return { requiredValidAttachment: true };
    }

    const hasValidAttachment = attachmentList.some(
      (attachment) => attachment.isDeleted !== true
    );

    return hasValidAttachment ? null : { requiredValidAttachment: true };
  };
}

import { AbstractControl, ValidatorFn } from '@angular/forms';
