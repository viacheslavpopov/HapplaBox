
import BaseElement from '@/components/BaseElement';
import styles from '@/components/thumbnail-bar/styles.inline.scss';
import template from '@/components/thumbnail-bar/template.html';
import ThumbnailItem from '@/components/thumbnail-bar/thumbnailItem';

const SELECTED_CLASS = 'selected';

const rootEl = document.createElement('template');
rootEl.innerHTML = template;

const styleEl = document.createElement('style');
styleEl.textContent = styles;

export class ThumbnailBar extends BaseElement {
  #multiselect = false;
  #items: ThumbnailItem[] = [];

  constructor() {
    super();

    // initialize component
    this.attachShadow({ mode: 'open' });
    this.shadowRoot.appendChild(styleEl);
    this.shadowRoot.appendChild(rootEl.content.cloneNode(true));

    // bind events
    this.onMouseWheel = this.onMouseWheel.bind(this);
    this.onKeyDown = this.onKeyDown.bind(this);

    // bind methods
    this.getItem = this.getItem.bind(this);
    this.selectItem = this.selectItem.bind(this);
    this.scrollToItem = this.scrollToItem.bind(this);
    this.scrollToSelectedItem = this.scrollToSelectedItem.bind(this);

    // register events
    const listEl = this.shadowRoot.querySelector('.thumbnail-list');
    listEl.addEventListener('wheel', this.onMouseWheel, false);
    listEl.addEventListener('keydown', this.onKeyDown, false);
  }


  private connectedCallback() {
    this.style.setProperty('--hostOpacity', '1');
  }

  get multiselect() {
    return this.#multiselect;
  }

  set multiselect(value: boolean) {
    this.#multiselect = value;
  }

  get items() {
    return this.#items;
  }

  set items(value: ThumbnailItem[]) {
    this.#items = value;
  }

  private onMouseWheel(e: WheelEvent) {
    // direction is vertical (mouse wheel)
    if (e.deltaX === 0) {
      e.preventDefault();

      // enable horizontal scrolling on mouse wheel
      // @ts-ignore
      e.currentTarget.scrollLeft += e.deltaY;
    }
  }

  private onKeyDown(e: KeyboardEvent) {
    if (e.key === 'Enter') {
      this.selectItem(this.shadowRoot.activeElement);
      this.scrollToItem(this.shadowRoot.activeElement);
    }
  }


  /**
   * Get the given element or item index
   * @returns Element | null
   */
  public getItem(value: Element | number) {
    if (Number.isSafeInteger(value)) {
      return this.shadowRoot.querySelector(`.thumbnail-list > li:nth-child(${value})`);
    }

    if (typeof value === 'object') {
      return value;
    }

    return null;
  }

  /**
   * Select the given element or item index
   */
  public selectItem(index: Element | number) {
    const el = this.getItem(index);
    if (!el) return;

    if (!this.#multiselect) {
      const allSelections = this.shadowRoot.querySelectorAll(`.${SELECTED_CLASS}`);
      allSelections.forEach(item => item.classList.remove(SELECTED_CLASS));
    }

    el.classList.add(SELECTED_CLASS);
  }

  /**
   * Scroll to the given element or item index
   */
  public scrollToItem(value: Element | number) {
    const el = this.getItem(value);
    if (!el) return;

    el.scrollIntoView({
      behavior: 'smooth',
      block: 'center',
      inline: 'center',
    });
  }

  /**
   * Scroll the the selected item. If multiple items selected, the last one will be chosen.
   */
  public scrollToSelectedItem() {
    const allSelections = Array.from(this.shadowRoot.querySelectorAll(`.${SELECTED_CLASS}`));
    const lastItem = allSelections.pop();

    this.scrollToItem(lastItem);
  }
}


export const initThumbnailBar = () => window.customElements.define('thumbnail-bar', ThumbnailBar);
