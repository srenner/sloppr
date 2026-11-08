import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  computed,
  input,
} from '@angular/core';
import { marked } from 'marked';

@Component({
  selector: 'app-markdown-view',
  templateUrl: './markdown-view.html',
  styleUrl: './markdown-view.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // The nodes come from [innerHTML], so emulated encapsulation attributes never
  // land on them. Styles are scoped by the .markdown-body wrapper instead.
  encapsulation: ViewEncapsulation.None,
})
export class MarkdownView {
  readonly markdown = input.required<string>();

  protected readonly html = computed(
    () => marked.parse(this.markdown(), { async: false }) as string,
  );
}
