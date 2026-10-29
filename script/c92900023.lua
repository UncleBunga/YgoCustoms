-- Slasher - Cannibal Patriarch
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    c:EnableReviveLimit()

    -- Cannot be Normal Summoned/Set
    local e0=Effect.CreateEffect(c)
    e0:SetType(EFFECT_TYPE_SINGLE)
    e0:SetCode(EFFECT_CANNOT_SUMMON)
    c:RegisterEffect(e0)
    local e0b=e0:Clone()
    e0b:SetCode(EFFECT_CANNOT_MSET)
    c:RegisterEffect(e0b)

    -- Special Summon procedure from hand by Tributing 1 "Cannibal" monster
    local e1=Effect.CreateEffect(c)
    e1:SetType(EFFECT_TYPE_FIELD)
    e1:SetCode(EFFECT_SPSUMMON_PROC)
    e1:SetProperty(EFFECT_FLAG_UNCOPYABLE)
    e1:SetRange(LOCATION_HAND)
    e1:SetCondition(s.spcon)
    e1:SetOperation(s.spop)
    c:RegisterEffect(e1)

    -- If Special Summoned: Activate 1 "Slasher" Field Spell from Deck or GY
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,0))
    e2:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e2:SetProperty(EFFECT_FLAG_DELAY)
    e2:SetCode(EVENT_SPSUMMON_SUCCESS)
    e2:SetCountLimit(1,id)
    e2:SetTarget(s.fstinftg)
    e2:SetOperation(s.fstinflop)
    c:RegisterEffect(e2)

    -- Float when destroyed by opponent's card
    local e3=Effect.CreateEffect(c)
    e3:SetDescription(aux.Stringid(id,1))
    e3:SetCategory(CATEGORY_SPECIAL_SUMMON)
    e3:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e3:SetProperty(EFFECT_FLAG_DELAY)
    e3:SetCode(EVENT_DESTROYED)
    e3:SetCountLimit(1,{id,1})
    e3:SetCondition(s.flcon)
    e3:SetTarget(s.fltg)
    e3:SetOperation(s.flop)
    c:RegisterEffect(e3)
end

-- Special Summon Condition: Tribute 1 "Cannibal" monster (0x1991)
function s.spfilter(c)
    return c:IsSetCard(0x1991)
end

function s.spcon(e,c)
    if c==nil then return true end
    local tp=c:GetControler()
    return Duel.CheckReleaseGroup(tp,s.spfilter,1,false,1,true,c,tp,nil,false,nil)
end

function s.spop(e,tp,eg,ep,ev,re,r,rp,c)
    local g=Duel.SelectReleaseGroup(tp,s.spfilter,1,1,false,1,true,c,tp,nil,false,nil)
    Duel.Release(g,REASON_COST)
end

-- Field Spell activation filter (0x991)
function s.fieldfilter(c,tp)
    return c:IsSetCard(0x991) and c:IsType(TYPE_FIELD) and c:GetActivateEffect():IsActivatable(tp,true,true)
end

function s.fstinftg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.fieldfilter,tp,LOCATION_DECK+LOCATION_GRAVE,0,1,nil,tp) end
end

function s.fstinflop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TOFIELD)
    local g=Duel.SelectMatchingCard(tp,aux.NecroValleyFilter(s.fieldfilter),tp,LOCATION_DECK+LOCATION_GRAVE,0,1,1,nil,tp)
    local tc=g:GetFirst()
    if tc then
        local fc=Duel.GetFieldCard(tp,LOCATION_SZONE,5)
        if fc then
            Duel.SendtoGrave(fc,REASON_RULE)
            Duel.BreakEffect()
        end
        Duel.ActivateFieldSpell(tc,e,tp,eg,ep,ev,re,r,rp)
    end
end

-- Float Condition: Destroyed by opponent's card in owner's control
function s.flcon(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    return c:IsPreviousControler(tp) and c:IsPreviousLocation(LOCATION_ONFIELD) and rp==1-tp
end

-- Filter for up to 3 Level 6 or lower "Cannibal" monsters
function s.flfilter(c,e,tp)
    return c:IsSetCard(0x1991) and c:IsLevelBelow(6) and c:IsCanBeSpecialSummoned(e,0,tp,false,false,POS_FACEUP)
end

function s.fltg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then
        return Duel.GetLocationCount(tp,LOCATION_MZONE)>0
        and Duel.IsExistingMatchingCard(s.flfilter,tp,LOCATION_HAND+LOCATION_DECK+LOCATION_GRAVE,0,1,nil,e,tp)
    end
    Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_HAND+LOCATION_DECK+LOCATION_GRAVE)
end

function s.flop(e,tp,eg,ep,ev,re,r,rp)
    local ft=Duel.GetLocationCount(tp,LOCATION_MZONE)
    if ft<=0 then return end
    if Duel.IsPlayerAffectedByEffect(tp,CARD_BLUEEYES_SPIRIT) then ft=1 end
    ft=math.min(ft,3)

    local g=Duel.GetMatchingGroup(aux.NecroValleyFilter(s.flfilter),tp,LOCATION_HAND+LOCATION_DECK+LOCATION_GRAVE,0,nil,e,tp)
    if #g>0 then
        Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
        local sg=aux.SelectUnselectGroup(g,e,tp,1,ft,aux.dncheck,1,tp,HINTMSG_SPSUMMON)
        if #sg>0 then
            Duel.SpecialSummon(sg,0,tp,tp,false,false,POS_FACEUP)
        end
    end

    -- Extra Deck restriction for the remainder of the turn
    local e1=Effect.CreateEffect(e:GetHandler())
    e1:SetType(EFFECT_TYPE_FIELD)
    e1:SetCode(EFFECT_CANNOT_SPECIAL_SUMMON)
    e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
    e1:SetDescription(aux.Stringid(id,2))
    e1:SetTargetRange(1,0)
    e1:SetTarget(s.splimit)
    e1:SetReset(RESET_PHASE+PHASE_END)
    Duel.RegisterEffect(e1,tp)
end

function s.splimit(e,c,sump,sumtype,sumpos,targetp,se)
    return c:IsLocation(LOCATION_EXTRA)
end