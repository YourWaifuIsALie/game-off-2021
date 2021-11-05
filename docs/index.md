---
layout: custom
---

{% for category in site.category-list %}
# {{ category }}
<ul>
  {% for page in site.pages reversed %}
    {% for page-category in page.categories %}
      {% if page-category == category %}
        <li><a href="{{ site.baseurl }}{{ page.url }}">{{ page.title }}</a></li>
      {% endif %}
    {% endfor %}
  {% endfor %}

  {% for post in site.posts reversed %}
    {% for post-category in post.categories %}
      {% if post-category == category %}
        <li><a href="{{ site.baseurl }}{{ post.url }}">{{ post.title }}</a></li>
      {% endif %}
    {% endfor %}
  {% endfor %}
</ul>
{% endfor %}