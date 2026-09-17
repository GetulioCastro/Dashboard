(function () {
  'use strict';

  var $ = function (s) { return document.querySelector(s); };
  var $$ = function (s) { return Array.prototype.slice.call(document.querySelectorAll(s)); };

  var MESES = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun'];

  function fmt(v) { return v.toLocaleString('pt-BR'); }
  function fmtBRL(v) {
    return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', minimumFractionDigits: 2 });
  }
  function soma(vals) { return vals.reduce(function (a, b) { return a + b; }, 0); }
  function cumulate(vals) {
    var out = [], acc = 0;
    vals.forEach(function (v) { acc += v; out.push(acc); });
    return out;
  }

  /* ------------------------------------------------------------------ *
   * Dados ilustrativos — NENHUM valor é real. Identificados como tal.
   * ------------------------------------------------------------------ */
  var IND = {
    atendimentos: {
      titulo: 'Atendimentos',
      unidade: 'atendimentos',
      ref: 'Data do atendimento (RN-09)',
      categoria: 'assistencial',
      mostraCobertura: true,
      mostraContagem: true,
      placeholderTipo: 'Filtro "Tipo de atendimento": definição pendente (Q2 / T-02). Exibido apenas como conceito.',
      labels: MESES,
      series: [
        { nome: 'Particular', cor: '#0d4d8c', valores: [340, 355, 362, 380, 390, 410] },
        { nome: 'Convênio', cor: '#1b7eb4', valores: [820, 860, 905, 930, 1010, 1080] },
        { nome: 'SUS', cor: '#7bae3f', valores: [180, 195, 205, 212, 198, 224] }
      ],
      pie: {
        titulo: 'Distribuição por cobertura (período)',
        itens: [
          { nome: 'Particular', valor: 2237, cor: '#0d4d8c' },
          { nome: 'Convênio', valor: 5605, cor: '#1b7eb4' },
          { nome: 'SUS', valor: 1214, cor: '#7bae3f' }
        ]
      }
    },
    consultas: {
      titulo: 'Consultas',
      unidade: 'consultas',
      ref: 'Data da consulta',
      categoria: 'assistencial',
      mostraCobertura: true,
      mostraContagem: true,
      placeholderTipo: 'Distinção "Primeira consulta × Retorno": definição pendente (RN-28 / T-02). Ilustração fictícia abaixo.',
      labels: MESES,
      series: [
        { nome: 'Particular', cor: '#0d4d8c', valores: [70, 75, 78, 82, 80, 88] },
        { nome: 'Convênio', cor: '#1b7eb4', valores: [130, 140, 144, 150, 158, 170] },
        { nome: 'SUS', cor: '#7bae3f', valores: [30, 33, 31, 34, 32, 36] }
      ],
      pie: {
        titulo: 'Distribuição por cobertura (período)',
        itens: [
          { nome: 'Particular', valor: 473, cor: '#0d4d8c' },
          { nome: 'Convênio', valor: 892, cor: '#1b7eb4' },
          { nome: 'SUS', valor: 196, cor: '#7bae3f' }
        ]
      },
      extrasText: 'Ilustração fictícia: 62% primeiras consultas, 38% retornos. Definição pendente (RN-28).'
    },
    exames: {
      titulo: 'Exames',
      unidade: 'exames',
      ref: 'Data do exame',
      categoria: 'assistencial',
      mostraCobertura: true,
      mostraContagem: true,
      mostraGrupoExame: true,
      labels: MESES,
      series: [
        { nome: 'Particular', cor: '#0d4d8c', valores: [95, 100, 104, 108, 106, 114] },
        { nome: 'Convênio', cor: '#1b7eb4', valores: [210, 220, 232, 240, 250, 262] },
        { nome: 'SUS', cor: '#7bae3f', valores: [55, 58, 60, 62, 60, 66] }
      ],
      pie: {
        titulo: 'Distribuição por grupo de exame (período, fictício)',
        itens: [
          { nome: 'Imagem', valor: 1105, cor: '#0d4d8c' },
          { nome: 'Laboratório', valor: 913, cor: '#1b7eb4' },
          { nome: 'Outros', valor: 384, cor: '#7bae3f' }
        ]
      },
      grupoExameNote: 'Grupo de exame conforme modelo do novo SisacHTML5 (RN-29 / RN-30). Valores fictícios.'
    },
    faturamento: {
      titulo: 'Faturamento',
      unidade: 'reais',
      ref: 'Data de emissão da guia/conta (RN-13)',
      categoria: 'faturamento',
      mostraFatura: true,
      mostraConvenio: true,
      labels: MESES,
      series: [
        { nome: 'Faturadas', cor: '#0d4d8c', valores: [380, 410, 395, 450, 430, 470] },
        { nome: 'A faturar', cor: '#e8a33d', valores: [22, 28, 26, 31, 27, 30] }
      ],
      pie: {
        titulo: 'Faturadas × A faturar (período)',
        itens: [
          { nome: 'Contas faturadas', valor: 2535, cor: '#0d4d8c' },
          { nome: 'Contas a faturar', valor: 164, cor: '#e8a33d' }
        ]
      },
      comparativo: { atual: 470, anterior: 430 },
      conveniosFicticios: ['Todos os convênios', 'AMIL', 'UNIMED', 'Bradesco Saúde', 'SUS'],
      nota: 'Estados de faturamento do novo banco (RN-33) ainda em definição — exibição conceitual de "faturadas / a faturar" sem fórmulas.'
    },
    producao: {
      titulo: 'Produção Médica',
      unidade: 'produções',
      ref: 'Período de referência conforme regra (a definir — T-02)',
      categoria: 'producao',
      mostraCobertura: true,
      mostraProfissional: true,
      labels: MESES,
      series: [
        { nome: 'Particular', cor: '#0d4d8c', valores: [340, 355, 362, 380, 390, 410] },
        { nome: 'Convênio', cor: '#1b7eb4', valores: [820, 860, 905, 930, 1010, 1080] },
        { nome: 'SUS', cor: '#7bae3f', valores: [180, 195, 205, 212, 198, 224] }
      ],
      pie: {
        titulo: 'Produção por cobertura (período)',
        itens: [
          { nome: 'Particular', valor: 2237, cor: '#0d4d8c' },
          { nome: 'Convênio', valor: 5605, cor: '#1b7eb4' },
          { nome: 'SUS', valor: 1214, cor: '#7bae3f' }
        ]
      },
      tabelaProf: {
        colunas: ['Particular', 'Convênio', 'SUS'],
        linhas: [
          { nome: 'Dr. Paulo Mendes', valores: [98, 230, 36] },
          { nome: 'Dr. Ana Lima', valores: [84, 210, 30] },
          { nome: 'Dr. Carlos Souza', valores: [76, 205, 42] },
          { nome: 'Dr. Beatriz Rocha', valores: [90, 188, 28] },
          { nome: 'Dra. Mariana Costa', valores: [62, 152, 24] }
        ]
      },
      nota: 'Métrica de produtividade e definição de "profissional ativo" em definição (RN-16 / RN-34; Q3 — T-02). Valores ilustrativos.'
    },
    despesas: {
      titulo: 'Despesas',
      unidade: 'reais',
      ref: 'Data de referência (competência × pagamento — a definir, T-02)',
      categoria: 'despesas',
      mostraDespesas: true,
      labels: MESES,
      series: [
        { nome: 'Fixas', cor: '#0d4d8c', valores: [150, 150, 152, 152, 155, 155] },
        { nome: 'Variáveis', cor: '#e8a33d', valores: [62, 71, 58, 78, 66, 88] }
      ],
      pie: {
        titulo: 'Fixas × Variáveis (período)',
        itens: [
          { nome: 'Despesas fixas', valor: 914, cor: '#0d4d8c' },
          { nome: 'Despesas variáveis', valor: 423, cor: '#e8a33d' }
        ]
      },
      nota: 'Composição de categorias fixas/variáveis e origem no novo banco em definição (Q5 — T-02 / T-03). Valores ilustrativos.',
      composicao: { fixas: 155, variaveis: 88 }
    }
  };

  /* ----------------------- renderizador SVG ----------------------- */
  function lineSVG(cfg) {
    var W = 720, H = 300, ml = 56, mr = 16, mt = 16, mb = 46;
    var pw = W - ml - mr, ph = H - mt - mb;
    var n = cfg.labels.length;
    var maxv = Math.max.apply(null, cfg.series.reduce(function (a, s) { return a.concat(s.valores); }, []));
    if (!maxv) maxv = 1;
    var step = Math.max(1, Math.ceil(maxv / 4));
    var ymax = step * 4;
    var x = function (i) { return ml + (n > 1 ? i * (pw / (n - 1)) : pw / 2); };
    var y = function (v) { return mt + ph - (v / ymax) * ph; };
    var g = '';
    for (var t = 0; t <= 4; t++) {
      var val = Math.round(t * step), yy = y(val);
      g += '<line x1="' + ml + '" y1="' + yy + '" x2="' + (W - mr) + '" y2="' + yy + '" stroke="#e5e7eb"/>';
      g += '<text x="' + (ml - 8) + '" y="' + (yy + 4) + '" text-anchor="end" font-size="11" fill="#6b7280">' + fmtAx(val) + '</text>';
    }
    cfg.labels.forEach(function (lb, i) {
      g += '<text x="' + x(i) + '" y="' + (H - mb + 18) + '" text-anchor="middle" font-size="11" fill="#6b7280">' + lb + '</text>';
    });
    cfg.series.forEach(function (s) {
      var pts = s.valores.map(function (v, i) { return x(i) + ',' + y(v); }).join(' ');
      g += '<polyline points="' + pts + '" fill="none" stroke="' + s.cor + '" stroke-width="2.5"/>';
      s.valores.forEach(function (v, i) {
        g += '<circle cx="' + x(i) + '" cy="' + y(v) + '" r="3.5" fill="' + s.cor + '"/>';
      });
    });
    return '<svg viewBox="0 0 ' + W + ' ' + H + '" role="img" class="w-100" style="max-height:320px">' + g + '</svg>';
  }

  function barSVG(cfg) {
    var W = 720, H = 300, ml = 56, mr = 16, mt = 16, mb = 46;
    var pw = W - ml - mr, ph = H - mt - mb;
    var n = cfg.labels.length, k = cfg.series.length;
    var maxv = Math.max.apply(null, cfg.series.reduce(function (a, s) { return a.concat(s.valores); }, []));
    if (!maxv) maxv = 1;
    var step = Math.max(1, Math.ceil(maxv / 4));
    var ymax = step * 4;
    var gw = pw / n;
    var bw = Math.min((gw * 0.75) / k, 46);
    var gap = (gw * 0.25) / k;
    var y = function (v) { return mt + ph - (v / ymax) * ph; };
    var g = '';
    for (var t = 0; t <= 4; t++) {
      var val = Math.round(t * step), yy = y(val);
      g += '<line x1="' + ml + '" y1="' + yy + '" x2="' + (W - mr) + '" y2="' + yy + '" stroke="#e5e7eb"/>';
      g += '<text x="' + (ml - 8) + '" y="' + (yy + 4) + '" text-anchor="end" font-size="11" fill="#6b7280">' + fmtAx(val) + '</text>';
    }
    cfg.labels.forEach(function (lb, i) {
      g += '<text x="' + (ml + i * gw + gw / 2) + '" y="' + (H - mb + 18) + '" text-anchor="middle" font-size="11" fill="#6b7280">' + lb + '</text>';
    });
    var totalW = k * bw + (k - 1) * gap;
    cfg.series.forEach(function (s, si) {
      s.valores.forEach(function (v, i) {
        var bx = ml + i * gw + gw / 2 - totalW / 2 + si * (bw + gap);
        var hval = (v / ymax) * ph;
        g += '<rect x="' + bx + '" y="' + y(v) + '" width="' + bw + '" height="' + Math.max(hval > 0 ? 2 : 0, hval) + '" fill="' + s.cor + '" rx="2"/>';
      });
    });
    g += '<line x1="' + ml + '" y1="' + (mt + ph) + '" x2="' + (W - mr) + '" y2="' + (mt + ph) + '" stroke="#9ca3af"/>';
    return '<svg viewBox="0 0 ' + W + ' ' + H + '" role="img" class="w-100" style="max-height:340px">' + g + '</svg>';
  }

  function fmtAx(v) {
    return v >= 1000 ? (v / 1000).toLocaleString('pt-BR') + 'k' : String(v);
  }

  function pieSVG(pie) {
    var W = 720, H = 300, cx = 150, cy = 150, r = 90;
    var total = pie.itens.reduce(function (a, it) { return a + it.valor; }, 0) || 1;
    var a = -Math.PI / 2, g = '';
    pie.itens.forEach(function (it) {
      var ang = (it.valor / total) * Math.PI * 2;
      var a1 = a + ang;
      var large = (a1 - a) > Math.PI ? 1 : 0;
      var x0 = cx + r * Math.cos(a), y0 = cy + r * Math.sin(a);
      var x1 = cx + r * Math.cos(a1), y1 = cy + r * Math.sin(a1);
      g += '<path d="M' + cx + ' ' + cy + ' L' + x0.toFixed(2) + ' ' + y0.toFixed(2) +
        ' A' + r + ' ' + r + ' 0 ' + large + ' 1 ' + x1.toFixed(2) + ' ' + y1.toFixed(2) + ' Z" fill="' + it.cor + '" stroke="#fff" stroke-width="1.5"/>';
      a = a1;
    });
    var ly = 62;
    pie.itens.forEach(function (it) {
      var pct = (it.valor / total * 100).toFixed(1).replace('.', ',');
      g += '<rect x="320" y="' + (ly - 11) + '" width="12" height="12" rx="2" fill="' + it.cor + '"/>';
      g += '<text x="340" y="' + ly + '" font-size="12" fill="#1f2937">' + it.nome + '</text>';
      g += '<text x="656" y="' + ly + '" text-anchor="end" font-size="12" fill="#1f2937">' + fmt(it.valor) + ' (' + pct + '%)</text>';
      ly += 24;
    });
    return '<svg viewBox="0 0 ' + W + ' ' + H + '" role="img" class="w-100" style="max-height:340px">' + g + '</svg>';
  }

  function legendHTML(series) {
    return series.map(function (s) {
      return '<span class="legend-item me-3"><span class="swatch" style="background:' + s.cor + '"></span>' + s.nome + '</span>';
    }).join('');
  }

  /* ----------------------- estado do protótipo ----------------------- */
  var state = {
    indId: 'atendimentos',
    periodo: 'atual',
    cobertura: 'todas',
    contagem: 'individual',
    fatura: 'Faturadas',
    despesas: 'todas',
    convenio: 'Todos os convênios',
    profissional: 'todos',
    grafico: 'linha'
  };

  function setDemo(which, names) {
    names.forEach(function (n) {
      var el = document.getElementById('demo-' + n);
      if (el) el.classList.toggle('d-none', n !== which);
    });
    $$('[data-demo-btn]').forEach(function (b) {
      b.classList.toggle('active', b.getAttribute('data-demo-btn') === which);
    });
  }

  function bindSeg(sel, cb) {
    $$(sel + ' [data-val]').forEach(function (b) {
      b.addEventListener('click', function () {
        $$(sel + ' [data-val]').forEach(function (x) { x.classList.remove('active'); });
        b.classList.add('active');
        cb(b.getAttribute('data-val'), b);
      });
    });
  }

  /* ----------------------- tela do indicador ----------------------- */
  function seriesParaGrafico(conf) {
    var series = conf.series.slice();
    if (conf.categoria === 'assistencial' || conf.categoria === 'producao') {
      if (state.cobertura !== 'todas') series = series.filter(function (s) { return s.nome === state.cobertura; });
    }
    if (conf.categoria === 'faturamento') {
      series = conf.series.filter(function (s) { return s.nome === state.fatura; });
    }
    if (conf.categoria === 'despesas') {
      if (state.despesas === 'fixas') series = conf.series.filter(function (s) { return s.nome === 'Fixas'; });
      else if (state.despesas === 'variaveis') series = conf.series.filter(function (s) { return s.nome === 'Variáveis'; });
    }
    if (conf.categoria === 'assistencial' && state.contagem === 'acumulada') {
      series = series.map(function (s) {
        return { nome: s.nome, cor: s.cor, valores: cumulate(s.valores) };
      });
    }
    return series;
  }

  function renderGrafico(conf) {
    var box = $('#grafico');
    var legenda = $('#legendaGrafico');
    var boxTipo = $('#tituloGrafico');
    if (!box) return;
    if (state.grafico === 'pizza') {
      box.innerHTML = pieSVG(conf.pie);
      boxTipo.textContent = conf.pie.titulo;
      legenda.innerHTML = '';
    } else {
      var series = seriesParaGrafico(conf);
      box.innerHTML = (state.grafico === 'coluna' ? barSVG({ labels: conf.labels, series: series })
                                                    : lineSVG({ labels: conf.labels, series: series }));
      boxTipo.textContent = state.grafico === 'coluna' ? 'Evolução mensal — colunas' : 'Evolução mensal — linhas';
      legenda.innerHTML = legendHTML(series);
    }
  }

  function resumoHtml(conf) {
    if (conf.tabelaProf) return [tableProfHTML(conf), conf.nota ? notaBox(conf.nota) : ''].join('');
    var money = conf.unidade === 'reais';
    var fmtCell = function (v) { return money ? fmtBRL(v * 1000) : fmt(v); };
    var rows = conf.series.map(function (s) {
      return '<tr><td>' + s.nome + '</td><td class="text-end">' + fmtCell(soma(s.valores)) + '</td></tr>';
    }).join('');
    var tot = soma(conf.series.map(function (s) { return soma(s.valores); }));
    var extra = conf.extrasText ? '<div class="alert alert-warning py-2 mt-3 mb-0 notifica">' + conf.extrasText + '</div>' : '';
    var nota = conf.nota ? notaBox(conf.nota) : '';
    return '<div class="table-responsive"><table class="table table-sm mb-0"><thead><tr><th>Categoria</th><th class="text-end">Total no período</th></tr></thead><tbody>' +
      rows + '<tr class="table-light"><td><strong>Total</strong></td><td class="text-end"><strong>' + fmtCell(tot) + '</strong></td></tr></tbody></table></div>' + extra + nota;
  }

  function tableProfHTML(conf) {
    var tp = conf.tabelaProf;
    var sel = state.profissional;
    var linhas = sel === 'todos' ? tp.linhas : tp.linhas.filter(function (l) { return l.nome === sel; });
    var head = '<th class="text-end">Total</th>';
    var body = linhas.map(function (l) {
      var tot = soma(l.valores);
      var cols = l.valores.map(function (v, i) { return '<td class="text-end">' + fmt(v) + '</td>'; }).join('');
      return '<tr><td>' + l.nome + '</td>' + cols + '<td class="text-end"><strong>' + fmt(tot) + '</strong></td></tr>';
    }).join('');
    var thead = '<tr><th>Profissional</th>' + tp.colunas.map(function (c) { return '<th class="text-end">' + c + '</th>'; }).join('') + head + '</tr>';
    return '<div class="table-responsive"><table class="table table-sm mb-0"><thead>' + thead + '</thead><tbody>' + body + '</tbody></table></div>';
  }

  function notaBox(msg) {
    return '<div class="alert alert-info mt-3 mb-0 nota-ficticia"><strong>Protótipo:</strong> ' + msg + '</div>';
  }

  function renderResumo(conf) {
    var el = $('#resumoInd');
    if (el) el.innerHTML = resumoHtml(conf);
    var comp = $('#comparativoPeriodo');
    if (comp) {
      if (conf.categoria === 'faturamento') {
        var c = conf.comparativo;
        var delta = ((c.atual - c.anterior) / c.anterior * 100).toFixed(1).replace('.', ',');
        comp.innerHTML =
          '<div class="d-flex flex-wrap gap-3">' +
          '<div><div class="nota-ficticia">Período atual (fictício)</div><div class="fs-4 fw-bold">' + fmtBRL(c.atual * 1000) + '</div></div>' +
          '<div><div class="nota-ficticia">Período anterior</div><div class="fs-4">' + fmtBRL(c.anterior * 1000) + '</div></div>' +
          '<div><div class="nota-ficticia">Variação (ilustrativa — sem fórmula)</div><div class="fs-4 text-success">+' + delta + '%</div></div>' +
          '</div>';
      } else {
        comp.innerHTML = '';
      }
    }
  }

  function configurarFiltros(conf) {
    var fCob = $('#filtroCobertura');
    var fCont = $('#filtroContagem');
    var fFat = $('#filtroFatura');
    var fConv = $('#selConvenioWrap');
    var fProf = $('#filtroProfissional');
    var fDesp = $('#filtroDespesas');
    var fGrupo = $('#filtroGrupoExame');
    var boxTipo = $('#boxPlaceholderTipo');

    if (fCob) fCob.classList.toggle('d-none', !conf.mostraCobertura);
    if (fCont) fCont.classList.toggle('d-none', !conf.mostraContagem);
    if (fFat) fFat.classList.toggle('d-none', !conf.mostraFatura);
    if (fConv) fConv.classList.toggle('d-none', !conf.mostraConvenio);
    if (fProf) fProf.classList.toggle('d-none', !conf.mostraProfissional);
    if (fDesp) fDesp.classList.toggle('d-none', !conf.mostraDespesas);
    if (fGrupo) fGrupo.classList.toggle('d-none', !conf.mostraGrupoExame);
    if (boxTipo) {
      if (conf.placeholderTipo) {
        boxTipo.classList.remove('d-none');
        boxTipo.querySelector('small').textContent = conf.placeholderTipo;
      } else {
        boxTipo.classList.add('d-none');
      }
    }
    if (conf.mostraConvenio) {
      var sel = $('#selConvenio');
      sel.innerHTML = conf.conveniosFicticios.map(function (c) {
        return '<option' + (c === 'Todos os convênios' ? ' selected' : '') + '>' + c + '</option>';
      }).join('');
    }
    if (conf.mostraProfissional) {
      var selP = $('#selProfissional');
      selP.innerHTML = '<option value="todos">Todos os profissionais</option>' + conf.tabelaProf.linhas.map(function (l) {
          return '<option value="' + l.nome + '">' + l.nome + '</option>';
        }).join('');
    }
    reaplicarVazio();
  }

  function reaplicarVazio() {
    /* no-op: mantido para simetria com o fluxo de render */
  }

  function atualizarPeriodo() {
    var lbl = $('#lblPeriodo');
    var boxDatas = $('#boxDatasPeriodo');
    if (!lbl) return;
    var ini = $('#dtIni').value, fim = $('#dtFim').value;
    var txt;
    if (state.periodo === 'atual') {
      boxDatas.classList.add('d-none');
      txt = 'Período atual — mês corrente (junho/2026). Exibido como: ';
    } else {
      boxDatas.classList.remove('d-none');
      txt = (state.periodo === 'passado' ? 'Período passado' : 'Período futuro') +
        ', com data inicial e data final. Exibido como: ';
    }
    lbl.textContent = txt + (ini && fim ? ini.replace(/-/g, '/') + ' a ' + fim.replace(/-/g, '/') : '(selecione as datas)') +
      ' — dados do protótipo não variam com a data (ilustrativos).';
  }

  function initIndicador() {
    var id = (location.hash ? location.hash.replace('#', '') : 'atendimentos');
    if (!IND[id]) id = 'atendimentos';
    state.indId = id;
    var conf = IND[id];

    $('#tituloInd').textContent = conf.titulo;
    $('#breadcrumbInd').textContent = conf.titulo;
    $('#refInd').textContent = 'Referência de negócio: ' + conf.ref;
    $('#unidadeInd').textContent = 'Unidade: ' + conf.unidade;

    configurarFiltros(conf);

    bindSeg('#segPeriodo', function (v) {
      state.periodo = v;
      var defaults = { atual: ['2026-06-01', '2026-06-30'], passado: ['2025-01-01', '2025-12-31'], futuro: ['2026-07-01', '2026-12-31'] };
      $('#dtIni').value = defaults[v][0];
      $('#dtFim').value = defaults[v][1];
      atualizarPeriodo();
    });
    $('#dtIni').addEventListener('change', atualizarPeriodo);
    $('#dtFim').addEventListener('change', atualizarPeriodo);

    bindSeg('#segCobertura', function (v) { state.cobertura = v; renderGrafico(conf); renderResumo(conf); });
    bindSeg('#segContagem', function (v) { state.contagem = v; renderGrafico(conf); });
    bindSeg('#segFatura', function (v) { state.fatura = v; renderGrafico(conf); });
    bindSeg('#segDespesas', function (v) { state.despesas = v; renderGrafico(conf); renderResumo(conf); });
    bindSeg('#segGrafico', function (v) { state.grafico = v; renderGrafico(conf); });

    var selConv = $('#selConvenio');
    if (selConv) selConv.addEventListener('change', function () {
      state.convenio = selConv.value;
      var nota = $('#lblConvenio');
      if (nota) nota.textContent = 'Convênio selecionado (fictício, lista ilustrativa): ' + state.convenio;
    });

    var selProf = $('#selProfissional');
    if (selProf) selProf.addEventListener('change', function () { state.profissional = selProf.value; renderResumo(conf); });

    $('#btnLimparFiltros').addEventListener('click', function () {
      state.cobertura = 'todas';
      state.contagem = 'individual';
      state.fatura = 'Faturadas';
      state.despesas = 'todas';
      state.profissional = 'todos';
      reaplicarSelecoes();
      renderGrafico(conf);
      renderResumo(conf);
      setDemo('default', ['default', 'carregando', 'vazio', 'erro', 'lento']);
    });

    atualizarPeriodo();
    renderGrafico(conf);
    renderResumo(conf);
  }

  function reaplicarSelecoes() {
    $$('[data-val]').forEach(function (b) {
      var ok =
        (b.closest('#segCobertura') && b.getAttribute('data-val') === 'todas') ||
        (b.closest('#segContagem') && b.getAttribute('data-val') === 'individual') ||
        (b.closest('#segFatura') && b.getAttribute('data-val') === 'Faturadas') ||
        (b.closest('#segDespesas') && b.getAttribute('data-val') === 'todas') ||
        (b.closest('#segGrafico') && b.getAttribute('data-val') === 'linha') ||
        (b.closest('#segPeriodo') && b.getAttribute('data-val') === 'atual');
      b.classList.toggle('active', ok);
    });
    var selP = $('#selProfissional');
    if (selP) selP.value = 'todos';
  }

  document.addEventListener('DOMContentLoaded', function () {
    if ($('#pgIndicador')) initIndicador();
    if ($('#btnRetry')) $('#btnRetry').addEventListener('click', function () { setDemo('default', ['default', 'carregando', 'vazio', 'erro', 'lento']); });
    if ($('#btnRetryDash')) $('#btnRetryDash').addEventListener('click', function () { setDemo('default', ['default', 'carregando', 'vazio', 'erro']); });
  });

})();